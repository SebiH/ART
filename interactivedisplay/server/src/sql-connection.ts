import { ReplaySubject } from 'rxjs';
import { SqlColumnMapping, CategoricalSqlMapping, MetricSqlMapping, DataRepresentation } from './sql-mapping';

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

export class SqlConnection {

    private sqlConnection: sql.Connection;
    private status: Status = new Status();

    private idCounter: number = 0;
    private idTable: { [sess_id: string]: number } = {};

    public constructor(private mapping: SqlColumnMapping[]) {}

    public connect(config: any) {

        if (this.status.isConnected()) {
            console.error('Cannot connect to sql server: Already connected');
            return;
        }

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
    }

    public disconnect(): void {
        if (this.status.isConnected()) {
            this.sqlConnection.close();
            this.status.set(ConnectionState.Offline);
        }
    }


    public getDimensions(): string[] {
        return <string[]> _.map(this.mapping, 'name');
    }

    public getData(dimension: string, onSuccess: (data: any[]) => void): void {
        this.status.whenReady(() => this.getDataConnectionEstablished(dimension, (data) => {
            onSuccess(data);
            // unmark connection from being busy, so that next request can be started
            this.status.set(ConnectionState.Connected);
        }));
    }

    // assumes connection is established
    private getDataConnectionEstablished(dimension: string, onSuccess: (data: any[]) => void): void {

        let mapping = _.find(this.mapping, map => map.name === dimension);

        if (!mapping) {
            console.error('Could not find database mapping of ' + dimension);
            return;
        }

        let requestedData: any[] = [];
        let filters: string[] = [];

        for (let map of this.mapping) {
            let min: any;
            let max: any;

            if (map.type == DataRepresentation.Categorical) {
                min = +_.minBy(map.values, 'dbValue').dbValue;
                max = +_.maxBy(map.values, 'dbValue').dbValue;
            } else {
                min = map.minValue;
                max = map.maxValue;

                if (map.dbTime) {
                    min = '\'' + this.formatDate(new Date(min * 1000)) + '\'';
                    max = '\'' + this.formatDate(new Date(max * 1000)) + '\'';
                }
            }

            filters.push(map.dbColumn + ' BETWEEN ' + min + ' AND ' + max);
            // filters.push(map.dbColumn + ' >= ' + min);
            // filters.push(map.dbColumn + ' <= ' + max);
        }

        let requestSql = '\
            SELECT TOP 1000 Sess_Id, ' + mapping.dbColumn + '\
            FROM Flat_Dataset_1';

        for (let i = 0; i < filters.length; i++) {
            requestSql += (i == 0) ? ' WHERE ' : ' AND ';
            requestSql += filters[i];
        }

        requestSql += ';';

        let request = new sql.Request(requestSql, (error: Error, rowCount: number, rows: any[]) => {
            if (error) {
                console.error('Could not complete request for ' + dimension);
                console.error(error);
            } else {
                onSuccess(requestedData);
            }
        });

        request.on('row', (columns) => {
            if (columns.length == 2) {
                requestedData.push({
                    id: this.idToNumber(columns[0].value),
                    value: mapping.converter(columns[1].value)
                });
            } else {
                console.error('Unexpected number of columns: Expected 2, got ' + columns.length);
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
