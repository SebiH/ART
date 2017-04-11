import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ReplaySubject } from 'rxjs/ReplaySubject';

import { ChartDimension } from '../models/index';

import * as _ from 'lodash';

@Injectable()
export class DataProvider {

    private dataCount: ReplaySubject<number> = new ReplaySubject<number>(1);
    private dimensions: ReplaySubject<string[]> = new ReplaySubject<string[]>(1);
    private data: { [id: string]: ReplaySubject<ChartDimension> } = {}

    constructor(private http: Http) {
        this.http.get('/api/graph/dimensions')
            .subscribe(res => this.dimensions.next(<string[]>res.json().dimensions));
    }

    public getDimensions(): Observable<string[]> {
        return this.dimensions.first();
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
