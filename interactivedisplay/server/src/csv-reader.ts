import { Parser } from 'csv-parse';
import { Observable, ReplaySubject } from 'rxjs';
import { SqlColumnMapping, CategoricalSqlMapping, MetricSqlMapping, DataRepresentation } from './sql-mapping';
import { RawData, RawDataPoint } from './raw-data';
import { DataSource } from './data-source';

import * as _ from 'lodash';
import * as fs from 'fs';

export interface CsvConfig {
    filename: string;
    options: any;
}

export class CsvReader implements DataSource {

    private cachedData: ReplaySubject<RawData[]>;

    public constructor(private config: CsvConfig, private mapping: SqlColumnMapping[]) {
        console.log('Using CSV data from ' + config.filename);
    }

    public getDimensions(): {name:string, displayName: string, phases:string[]}[] {
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

    public getData(): Observable<RawData[]> {
        if (!this.cachedData) {

            this.cachedData = new ReplaySubject<RawData[]>(1);

            // see http://csv.adaltas.com/parse/
            let parser = new Parser(this.config.options);
            let data: RawData[] = [];
            let dimensions: string[] = [];
            let idCounter = 0;

            parser.on('readable', () => {
                let record: any;
                while (record = parser.read()) {
                    if (dimensions.length == 0) {
                        dimensions = record;
                    } else {
                        let dims: { [dim: string]: RawDataPoint } = {};

                        for (let i = 0; i < dimensions.length; i++) {
                            let map = _.find(this.mapping, (m) => m.dbColumn == dimensions[i]);

                            if (map) {
                                dims[map.dbColumn] = {
                                    value: map.converter(record[i]),
                                    isNull: false
                                };
                            }
                        }

                        data.push({
                            // id: record[0],
                            id: idCounter++,
                            dimensions: dims
                        });
                    }
                }
            });

            parser.on('error', (err) => {
                console.error(err.message);
            });

            parser.on('finish', () => {
                this.cachedData.next(data);
            });


            fs.readFile(this.config.filename, 'utf8', (err, content) => {
                if (err) {
                    console.error(err);
                } else {
                    parser.write(content);
                }
                parser.end();
            });
        }

        return this.cachedData.asObservable();
    }
}
