import { SqlConnection, SqlData } from './sql-connection';
import { DataRepresentation } from './sql-mapping';
import { SmartactMapping } from './smartact-mappings';
import * as _ from 'lodash';

export class GraphDataProvider {

    private sqlConnection = new SqlConnection(SmartactMapping);
    private dataCache: { [id: string]: any } = {};

    public constructor(useRandom?: boolean) {
        let config = require('../sql.conf.json');

        if (config.debug) {
            console.log('Using random data');
            let randomDataCount = 1000;
            let data: SqlData[] = [];
            for (let i = 0; i < randomDataCount; i++) {
                data.push({ id: i, dimensions: {} });
            }

            for (let mapping of SmartactMapping) {
                let dimension = mapping.name;
                let minValue = (mapping.type === DataRepresentation.Categorical) ?
                    +_.minBy(mapping.values, 'dbValue').dbValue :
                    +mapping.minValue;
                let maxValue = (mapping.type === DataRepresentation.Categorical) ?
                    +_.maxBy(mapping.values, 'dbValue').dbValue :
                    +mapping.maxValue;

                for (let i = 0; i < randomDataCount; i++) {
                    let val = Math.random() * (maxValue - minValue) + minValue;
                    if (mapping.type === DataRepresentation.Categorical) {
                        val = Math.round(val);
                    }
                    data[i].dimensions[dimension] = val;
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

            this.sqlConnection.getData()
                .first()
                .subscribe((data) => {
                    this.dataCache[dimension] = this.convertData(dimension, data);
                    onDataRetrieved(this.dataCache[dimension]);
                });

        } else {
            onDataRetrieved(this.dataCache[dimension]);
        }
    }

    private convertData(dimension: string, data: SqlData[]): any {
        let mapping = _.find(SmartactMapping, m => m.name === dimension);

        if (!mapping) {
            console.log('Unable to find mapping for ' + dimension);
            return {};
        }

        let values: {id: string, value: number}[] = [];
        for (let datum of data) {
            values.push({
                id: '' + datum.id,
                value: datum.dimensions[dimension],
            });
        }

        let minValue = 0;
        let maxValue = 1;
        let isMetric = false;
        let isTimeBased = false;
        let timeFormat = "";
        let mappings: any[] | null = [];
        let bins: any[] | null = null;
        let gradient: any[] | null = null;
        let ticks: number[] | null = null;

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
                isTimeBased = mapping.isTimeBased;
                timeFormat = mapping.timeFormat || "";
                mappings = null;
                bins = mapping.bins;
                gradient = mapping.gradient;
                ticks = mapping.ticks;
                break;

            default:
                this.assertNever(mapping);
                break;
        }

        // calculate dynamic range
        if (mapping.type === DataRepresentation.Metric) {
            // start with opposite value, so that we can search for the actual min/max value
            let dynMinValue = maxValue;
            let dynMaxValue = minValue;

            for (let i = 0; i < values.length; i++) {
                dynMinValue = Math.min(dynMinValue, values[i].value);
                dynMaxValue = Math.max(dynMaxValue, values[i].value);
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
            isTimeBased: isTimeBased,
            timeFormat: timeFormat,
            mappings: mappings,
            bins: bins,
            gradient: gradient,
            ticks: ticks
        }
    }

    private assertNever(x: never): never {
        throw new Error('Unexpected object: ' + x);
    }
}
