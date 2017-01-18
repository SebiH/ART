import { ReplaySubject } from 'rxjs';
import { SqlColumnMapping, SmartactMapping } from './sql-mapping';

import * as sql from 'tedious';
import * as _ from 'lodash';

export class SqlConnection {

    private connectionSubject = new ReplaySubject<boolean>();
    private sqlConnection: sql.Connection;
    private isConnected: boolean = false;

    public connect() {

        if (this.isConnected) {
            console.error('Cannot connect to sql server: Already connected');
            return;
        }

        let config = require('../sql.conf.json');
        this.sqlConnection = new sql.Connection(config);

        this.sqlConnection.on('connect', (error) => {
            if (error) {
                console.error('Cannot connect to sql server: Connection terminated');
                console.error(error);
                this.isConnected = false;
                this.connectionSubject.next(false);
            } else {
                console.log('Established connection to SQL Server @ ' + config.server);
                this.isConnected = true;
                this.connectionSubject.next(true);
            }
        });
    }

    public disconnect(): void {
        if (this.isConnected) {
            this.sqlConnection.close();
            this.isConnected = false;
            this.connectionSubject.next(false);
        }
    }


    public getDimensions(): string[] {
        return <string[]> _.map(SmartactMapping, 'name');
    }

    public getData(dimension: string, onSuccess: (data: any[]) => void): void {
        this.connectionSubject
            .skipWhile(isConnected => !isConnected)
            .first()
            .subscribe(() => this.getDataConnectionEstablished(dimension, onSuccess));
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
            SELECT TOP 10 User_Id, ' + mapping.dbColumn + '\
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
