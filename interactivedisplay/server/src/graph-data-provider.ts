import { SqlConnection } from './sql-connection';
import { SmartactMapping, DataRepresentation } from './sql-mapping';
import * as _ from 'lodash';

export class GraphDataProvider {

    private sqlConnection = new SqlConnection();
    private dataCache: { [id: string]: any } = {};

    public constructor(useRandom?: boolean) {
        if (useRandom) {
            for (let mapping of SmartactMapping) {
                let dimension = mapping.name;
                let data = [];
                let minValue = (mapping.type === DataRepresentation.Categorical) ?
                    +_.minBy(mapping.values, 'dbValue').dbValue :
                    +mapping.minValue;
                let maxValue = (mapping.type === DataRepresentation.Categorical) ?
                    +_.maxBy(mapping.values, 'dbValue').dbValue :
                    +mapping.maxValue;

                for (let i = 0; i < 300; i++) {
                    let val = Math.random() * (maxValue - 1) + minValue;
                    if (mapping.type === DataRepresentation.Categorical) {
                        val = Math.round(val);
                    }
                    data.push({ value: val });
                }
                this.dataCache[dimension] = this.convertData(dimension, data);
            }
        } else {
            this.sqlConnection.connect();
        }
    }

    public getDimensions(): any {
        // workaround since Unity needs an object type for JSON conversion
        return { dimensions: this.sqlConnection.getDimensions() };
    }

    public getData(dimension: string, onDataRetrieved: (data: any) => void): void {
        if (this.dataCache[dimension] === undefined) {

            console.log('Loading \'' + dimension + '\' from sql server');
            this.sqlConnection.getData(dimension, (data) => {
                this.dataCache[dimension] = this.convertData(dimension, data);
                onDataRetrieved(this.dataCache[dimension]);
            });

        } else {
            console.log('Loading \'' + dimension + '\' from cache');
            onDataRetrieved(this.dataCache[dimension]);
        }
    }

    private convertData(dimension: string, data: any): any {
        let mapping = _.find(SmartactMapping, m => m.name === dimension);

        if (!mapping) {
            console.log('Unable to find mapping for ' + dimension);
            return {};
        }


        let values = _.map(data, 'value');
        let minValue = 0;
        let maxValue = 1;
        let isMetric = false;
        let mappings = [];

        switch (mapping.type) {
            case DataRepresentation.Categorical:
                minValue = +_.minBy(mapping.values, 'dbValue').dbValue;
                maxValue = +_.maxBy(mapping.values, 'dbValue').dbValue;
                isMetric = false;
                mappings = [];
                for (let m of mapping.values) {
                    mappings.push({ value: +m.dbValue, name: m.name });
                }
                break;

            case DataRepresentation.Metric:
                minValue = mapping.minValue;
                maxValue = mapping.maxValue;
                isMetric = true;
                mappings = null;
                break;

            default:
                this.assertNever(mapping);
                break;
        }

        // mirror chart-dimension model in web app
        return {
            data: values,
            domain: {
                min: minValue,
                max: maxValue
            },
            name: dimension,
            isMetric: isMetric,
            mappings: mappings
        }
    }

    private assertNever(x: never): never {
        throw new Error('Unexpected object: ' + x);
    }
}
