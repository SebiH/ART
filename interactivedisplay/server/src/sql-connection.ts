import { Observable, ReplaySubject } from 'rxjs';
import { SqlColumnMapping, CategoricalSqlMapping, MetricSqlMapping, DataRepresentation } from './sql-mapping';
import { RawData } from './raw-data';
import { DataSource } from './data-source';

import * as Colors from './colors';
import * as sql from 'tedious';
import * as _ from 'lodash';

enum ConnectionState {
    Offline, Connected, Busy
};

class Status {

    public subscription = new ReplaySubject<ConnectionState>();

    private connectionStatus = ConnectionState.Offline;

    public isConnected(): boolean {
        return this.connectionStatus === ConnectionState.Connected ||
            this.connectionStatus === ConnectionState.Busy;
    }

    public set(state: ConnectionState): void {
        this.connectionStatus = state;
        this.subscription.next(state);
    }

    public get(): ConnectionState {
        return this.connectionStatus;
    }

    public whenReady(fn: (state: ConnectionState) => void) {
        return this.subscription
            .skipWhile(status => {
                // query member instead, in case it changes to busy
                if (this.connectionStatus === ConnectionState.Connected) {
                    // only allow *one* connection at a time
                    this.connectionStatus = ConnectionState.Busy;
                    return false;
                }

                return true; // skip
            })
            .first()
            .subscribe(fn);
    }
}

export class SqlConnection implements DataSource {

    private sqlConnection: sql.Connection;
    private status: Status = new Status();

    private sqlQuery: string = "";
    private table: string = "";

    private sqlData: ReplaySubject<RawData[]> = new ReplaySubject<RawData[]>(1);
    private idCounter: number = 0;
    private readonly idTable: { [sess_id: string]: number } = {};

    public constructor(private mapping: SqlColumnMapping[]) {}

    public connect(config: any) {
        if (!config.table) {
            throw "No table found in config";
        } else {
            this.table = config.table;
            console.log("Using table " + config.table);
        }

        if (this.status.isConnected()) {
            console.error('Cannot connect to sql server: Already connected');
            return;
        }

        this.startConnection(config);
    }

    private startConnection(config: any) {
        this.sqlConnection = new sql.Connection(config);

        this.sqlConnection.on('connect', (error) => {
            if (error) {
                console.error('Cannot connect to sql server: Connection terminated');
                console.error(error);
                this.status.set(ConnectionState.Connected);
            } else {
                console.log('Established connection to SQL Server @ ' + config.server);
                this.status.set(ConnectionState.Connected);
            }
        });

        this.sqlConnection.on('error', (error) => {
            console.error('SqlConnection error, trying to reconnect:');
            console.error(error);
            this.status.set(ConnectionState.Offline);
            this.startConnection(config);
        });
    }

    public disconnect(): void {
        if (this.status.isConnected()) {
            this.sqlConnection.close();
            this.status.set(ConnectionState.Offline);
        }
    }


    public getDimensions(): { name: string, displayName: string, phases: string[] }[] {
        let dimensions: {name: string, displayName: string, phases: string[]}[] = [];
        for (let map of this.mapping) {
            dimensions.push({
                name: map.dbColumn,
                displayName: map.name,
                phases: map.phases
            });
        }

        return dimensions;
    }

    public setSqlQuery(query: string) {
        this.sqlQuery = query;

        this.status.whenReady(() => {
            this.getDataConnectionEstablished((data) => {
                this.sqlData.next(data);
                // unmark connection from being busy, so that next request can be started
                this.status.set(ConnectionState.Connected);
            });
        });
    }

    public getData(): Observable<RawData[]> {
        this.status.whenReady(() => {
            this.getDataConnectionEstablished((data) => {
                this.sqlData.next(data);
                // unmark connection from being busy, so that next request can be started
                this.status.set(ConnectionState.Connected);
            });
        });
        return this.sqlData.asObservable();
    }

    // assumes connection is established
    private getDataConnectionEstablished(onSuccess: (data: RawData[]) => void): void {

        let filters: string[] = [];

        for (let map of this.mapping) {

            if (map.type == DataRepresentation.Categorical) {

                if (!map.autoGenerateValues) {
                    let filter = map.dbColumn + ' IN (';

                    if (map.limitValues) {
                        for (let i = 0; i < map.limitValues.length; i++) {
                            if (i > 0) {
                                filter += ',';
                            }
                            filter += '' + map.limitValues[i];
                        }
                    } else {
                        for (let i = 0; i < map.values.length; i++) {
                            if (i > 0) {
                                filter += ',';
                            }
                            filter += '' + map.values[i].dbValue;
                        }
                    }

                    filter += ')';
                    filters.push(filter);
                }
            } else {
                let min: any = map.minValue;
                let max: any = map.maxValue;

                if (map.dbTime) {
                    min = '\'' + this.formatDate(new Date(min * 1000)) + '\'';
                    max = '\'' + this.formatDate(new Date(max * 1000)) + '\'';
                }

                filters.push(map.dbColumn + ' BETWEEN ' + min + ' AND ' + max);
            }

        }

        let requestSql = 'SELECT ';
        let isFirst = true;

        for (let map of this.mapping) {
            if (isFirst) { requestSql += map.dbColumn; isFirst = false; }
            else {
                requestSql += ', ' + map.dbColumn;
            }
        }

        requestSql += ' FROM ' + this.table + ' ';


        requestSql += ' WHERE name = \'AvgDay_BE_day\'';
        // for (let i = 0; i < filters.length; i++) {
        //     requestSql += (i == 0) ? ' WHERE ' : ' AND ';
        //     requestSql += filters[i];
        // }

        requestSql += ' ORDER BY Cond;';
        console.log(requestSql);

        // let requestSql = this.sqlQuery;


        let requestedData: RawData[] = [];
        let request = new sql.Request(requestSql, (error: Error, rowCount: number, rows: any[]) => {
            if (error) {
                console.error('Could not complete sql request');
                console.error(error);
            } else {
                onSuccess(requestedData);
            }
        });

        let idCounter = 0;

        request.on('row', (columns) => {
            if (columns.length == this.mapping.length) {
                let values: {[dim: string]: number} = { };

                for (let i = 0; i < this.mapping.length; i++) {
                    let map = this.mapping[i];
                    let value = columns[i].value;
                    let numericValue = map.converter(value);

                    if (map.type == DataRepresentation.Categorical && map.autoGenerateValues) {
                        let mapVal = _.find(map.values, v => v.dbValue == numericValue);
                        if (!mapVal) {
                            map.values.push({
                                dbValue: numericValue,
                                name: value,
                                color: Colors.random()
                            });
                        }
                    }

                    values[map.dbColumn] = numericValue;
                }

                requestedData.push({
                    id: idCounter++, //this.idToNumber(columns[0].value),
                    dimensions: values
                });
            } else {
                console.error('Unexpected number of columns: Expected ' + (this.mapping.length + 1) + ', got ' + columns.length);
            }
        });

        this.sqlConnection.execSql(request);
    }

    private formatDate(date: Date): string {
        let hours = '0' + date.getUTCHours();
        let minutes = '0' + date.getUTCMinutes();
        let seconds = '0' + date.getUTCSeconds();

        return hours.substr(-2) + ':' + minutes.substr(-2) + ':' + seconds.substr(-2) + '.0000000';
    }

    private idToNumber(sessId: string): number {
        if (this.idTable[sessId] === undefined) {
            this.idTable[sessId] = this.idCounter++;
        }

        return this.idTable[sessId];
    }
}
