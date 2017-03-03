import { SqlConnection } from './sql-connection';
import { SmartactMapping, DataRepresentation } from './sql-mapping';
import * as _ from 'lodash';

export class GraphDataProvider {

    private sqlConnection = new SqlConnection();
    private dataCache: { [id: string]: any } = {};

    public constructor(useRandom?: boolean) {
        let config = require('../sql.conf.json');

        if (config.debug) {
            console.log('Using random data');
            for (let mapping of SmartactMapping) {
                let dimension = mapping.name;
                let data: any[] = [];
                let minValue = (mapping.type === DataRepresentation.Categorical) ?
                    +_.minBy(mapping.values, 'dbValue').dbValue :
                    +mapping.minValue;
                let maxValue = (mapping.type === DataRepresentation.Categorical) ?
                    +_.maxBy(mapping.values, 'dbValue').dbValue :
                    +mapping.maxValue;

                for (let i = 0; i < 1000; i++) {
                    let val = Math.random() * (maxValue) + minValue;
                    if (mapping.type === DataRepresentation.Categorical) {
                        val = Math.floor(val);
                    }
                    data.push({ value: val });
                }
                this.dataCache[dimension] = this.convertData(dimension, data);
            }
        } else {
            this.sqlConnection.connect(config.sqlSecrets);
        }
    }

    public getDimensions(): any {
        // workaround since Unity needs an object type for JSON conversion
        return { dimensions: this.sqlConnection.getDimensions() };
    }

    public getData(dimension: string, onDataRetrieved: (data: any) => void): void {
        if (this.dataCache[dimension] === undefined) {

            this.sqlConnection.getData(dimension, (data) => {
                this.dataCache[dimension] = this.convertData(dimension, data);
                onDataRetrieved(this.dataCache[dimension]);
            });

        } else {
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
        let mappings: any[] | null = [];

        switch (mapping.type) {
            case DataRepresentation.Categorical:
                minValue = +_.minBy(mapping.values, 'dbValue').dbValue;
                maxValue = +_.maxBy(mapping.values, 'dbValue').dbValue;
                isMetric = false;
                mappings = [];
                for (let m of mapping.values) {
                    mappings.push({ value: +m.dbValue, name: m.name, color: m.color });
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

        // force-clamp values to their given domain (assuming data is number[])
        // TODO: falsifies data, but that's preferable to interface bugs atm!
        for (let i = 0; i < data.length; i++) {
            data[i].value = Math.max(minValue, Math.min(data[i].value, maxValue));
        }

        // calculate dynamic range
        if (mapping.type === DataRepresentation.Metric) {
            // start with opposite value, so that we can search for the actual min/max value
            let dynMinValue = maxValue;
            let dynMaxValue = minValue;

            for (let i = 0; i < data.length; i++) {
                dynMinValue = Math.min(dynMinValue, data[i].value);
                dynMaxValue = Math.max(dynMaxValue, data[i].value);
            }

            // let range = (maxValue - minValue);
            // let margin = range / 10;
            // if (range > 50000) {
            //     margin = range / 10000;
            // }
            let margin = 0;

            maxValue = Math.min(Math.ceil(dynMaxValue + margin), maxValue);
            minValue = Math.max(Math.floor(dynMinValue - margin), minValue);
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
