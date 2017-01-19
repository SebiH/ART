import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ReplaySubject } from 'rxjs/ReplaySubject';

import { ChartDimension } from '../models/index';

import * as _ from 'lodash';

@Injectable()
export class GraphDataProvider {

    private dimensions: ReplaySubject<string[]> = new ReplaySubject<string[]>(1);
    private data: { [id: string]: ReplaySubject<ChartDimension> } = {}

    constructor(private http: Http) {
        this.http.get('/api/graph/dimensions')
            .subscribe(res => this.dimensions.next(<string[]>res.json().dimensions));
    }

    public getDimensions(): ReplaySubject<string[]> {
        return this.dimensions;
    }

    public getData(dim: string): ReplaySubject<ChartDimension> {
        if (this.data[dim] === undefined) {
            let rs = new ReplaySubject<ChartDimension>(1);
            this.data[dim] = rs;
            this.http.post('/api/graph/data', { dimension: dim })
                .subscribe(res => {
                    rs.next(<ChartDimension>res.json())
                });
        }

        return this.data[dim];
    }
}
