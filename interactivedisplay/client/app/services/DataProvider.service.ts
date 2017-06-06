import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ReplaySubject } from 'rxjs/ReplaySubject';

import { ChartDimension } from '../models/index';

import * as _ from 'lodash';

export interface Dimension {
    name: string;
    phases: string[];
};

@Injectable()
export class DataProvider {

    private dataCount: ReplaySubject<number> = new ReplaySubject<number>(1);
    private dimensions: ReplaySubject<Dimension[]> = new ReplaySubject<Dimension[]>(1);
    private data: { [id: string]: ReplaySubject<ChartDimension> } = {}

    constructor(private http: Http) {
        this.http.get('/api/graph/dimensions')
            .subscribe(res => this.dimensions.next(<Dimension[]>res.json().dimensions));
    }

    public getDimensions(): Observable<Dimension[]> {
        return this.dimensions.first();
    }

    public getPhases(): Observable<string[]> {
        return this.dimensions
            .first()
            .map(dims => {
                let phases: string[] = [];
                for (let dim of dims) {
                    for (let phase of dim.phases) {
                        if (phases.indexOf(phase) < 0) {
                            phases.push(phase);
                        }
                    }
                }

                return phases;
            });
    }

    public getDimensionNamesByPhase(phase: string): Observable<string[]> {
        return this.dimensions
            .first()
            .map(dims => _.map(_.uniqBy(_.filter(dims, (d) => d.phases.indexOf(phase) >= 0), 'name'), 'name'));
    }

    public getDimensionNames(): Observable<string[]> {
        return this.dimensions
            .first()
            .map(dims => _.map(_.uniqBy(dims, 'name'), 'name'));
    }

    public getData(dim: string): ReplaySubject<ChartDimension> {
        if (this.data[dim] === undefined) {
            let rs = new ReplaySubject<ChartDimension>(1);
            this.data[dim] = rs;
            this.http.post('/api/graph/data', { dimension: dim })
                .subscribe(res => {
                    let chartDim = ChartDimension.fromJson(res.json());
                    rs.next(chartDim);
                    this.dataCount.next(chartDim.data.length);
                });
        }

        return this.data[dim];
    }

    public onDataCount(): Observable<number> {
        return this.dataCount.asObservable();
    }
}
