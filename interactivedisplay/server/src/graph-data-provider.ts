import { SqlConnection } from './sql-connection';
import * as _ from 'lodash';

export class GraphDataProvider {

    private sqlConnection = new SqlConnection();
    private dataCache: { [id: string]: any } = {};

    public constructor() {
        this.sqlConnection.connect();
    }

    public getDimensions(): any {
        // workaround since Unity needs an object type for JSON conversion
        return { dimensions: this.sqlConnection.getDimensions() };
    }

    public getData(dimension: string, onDataRetrieved: (data: any) => void): void {
        if (this.dataCache[dimension] === undefined) {
            console.log('Loading \'' + dimension + '\' from sql server');
            this.sqlConnection.getData(dimension, (data) => {
                let vals = { data: _.map(data, 'value') };
                this.dataCache[dimension] = vals;
                onDataRetrieved(vals);
            });
        } else {
            console.log('Loading \'' + dimension + '\' from cache');
            onDataRetrieved(this.dataCache[dimension]);
        }
    }
}
