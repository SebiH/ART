import { ReplaySubject } from 'rxjs';
import { SqlColumnMapping, SmartactMapping } from './sql-mapping';

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

    public connect() {

        if (this.status.isConnected()) {
            console.error('Cannot connect to sql server: Already connected');
            return;
        }

        let config = require('../sql.conf.json');
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
        return <string[]> _.map(SmartactMapping, 'name');
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

        let mapping = _.find(SmartactMapping, map => map.name === dimension);

        if (!mapping) {
            console.error('Could not find database mapping of ' + dimension);
            return;
        }

        let requestedData = [];
        let requestSql = '\
            SELECT TOP 1000 User_Id, ' + mapping.dbColumn + '\
            FROM Flat_Dataset_1';

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
                    value: +columns[1].value
                });
            } else {
                console.error('Unexpected number of columns: Expected 2, got ' + columns.length);
            }
        });

        this.sqlConnection.execSql(request);
    }


    private idToNumber(userId: string): number {
        return +userId.replace(/\./, '');
    }
}
