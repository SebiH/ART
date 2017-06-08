import { RawData } from './raw-data';
import { DataSource } from './data-source';
import { SqlConnection } from './sql-connection';
import { CsvReader, CsvConfig } from './csv-reader';
import { DataRepresentation } from './sql-mapping';
import { SmartactMapping } from './smartact-mappings';
import { SmartactTemporalMapping } from './smartact-temporal-mappings';
import { TitanicMapping } from './titanic-mappings';
import { SqlColumnMapping } from './sql-mapping';
import * as Colors from './colors';
import * as _ from 'lodash';

export class GraphDataProvider {

    private dataSource: DataSource;
    private dataCache: { [id: string]: any } = {};
    private mapping: SqlColumnMapping[];

    public constructor(config: any) {
        if (config.mode == "debug") {
            console.log('Using random data');
            this.mapping = SmartactTemporalMapping;
            let randomDataCount = 1000;
            let data: RawData[] = [];
            for (let i = 0; i < randomDataCount; i++) {
                data.push({ id: i, dimensions: {} });
            }

            for (let mapping of SmartactTemporalMapping) {
                let dimension = mapping.dbColumn;

                if (mapping.type == DataRepresentation.Categorical && mapping.autoGenerateValues) {
                    for (let i = 0; i < randomDataCount; i++) {
                        data[i].dimensions[mapping.dbColumn] = i;
                        mapping.values.push({
                            color: Colors.random(),
                            dbValue: i,
                            name: i + ''
                        });
                    }
                    this.dataCache[dimension] = this.convertData(dimension, data);
                } else {
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
            }
        } else if (config.mode == "sql") {
            let sqlConnection = new SqlConnection(SmartactTemporalMapping);
            sqlConnection.connect(config.sqlSecrets);
            this.dataSource = sqlConnection;
            this.mapping = SmartactTemporalMapping;
        } else if (config.mode == "csv") {
            this.dataSource = new CsvReader(config.csvConfig as CsvConfig, TitanicMapping);
            this.mapping = TitanicMapping;
        } else {
            throw new Error("Unknown config mode " + config.mode);
        }

        if (this.dataSource) {
            // load data on startup for faster response later on
            this.dataSource.getData()
                .first()
                .subscribe(data => console.log('Caching data completed'));

            if (this.dataSource instanceof SqlConnection) {
                this.dataSource.disconnect();
            }
        }
    }

    public getDataSource(): DataSource {
        return this.dataSource;
    }

    public getDimensions(): {dimensions: {name: string, displayName: string, phases: string[]}[]} {
        if (this.dataSource) {
            // workaround since Unity needs an object type for JSON conversion
            return { dimensions: this.dataSource.getDimensions() };
        } else {
            let dimensions: {name: string, displayName: string, phases: string[]}[] = [];
            for (let map of this.mapping) {
                dimensions.push({
                    name: map.dbColumn,
                    displayName: map.name,
                    phases: map.phases
                });
            }

            return {dimensions: dimensions};
        }
    }

    public getData(dimension: string, onDataRetrieved: (data: any) => void): void {
        if (this.dataCache[dimension] === undefined) {

            this.dataSource.getData()
                .first()
                .subscribe((data) => {
                    this.dataCache[dimension] = this.convertData(dimension, data);
                    onDataRetrieved(this.dataCache[dimension]);
                });

        } else {
            onDataRetrieved(this.dataCache[dimension]);
        }
    }

    private convertData(dimension: string, data: RawData[]): any {
        let mapping = _.find(this.mapping, m => m.dbColumn == dimension);

        if (!mapping) {
            console.log('Unable to find mapping for ' + dimension);
            return {};
        }

        let values: {id: string, value: number}[] = [];
        for (let datum of data) {
            values.push({
                id: '' + datum.id,
                value: datum.dimensions[mapping.dbColumn],
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
            name: mapping.dbColumn,
            displayName: mapping.name,
            hideTicks: !!mapping.hideTicks,
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
