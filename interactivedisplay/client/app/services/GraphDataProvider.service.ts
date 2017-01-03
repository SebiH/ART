import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ReplaySubject } from 'rxjs/ReplaySubject';

import { Http } from '@angular/http';
import * as _ from 'lodash';

@Injectable()
export class GraphDataProvider {

    public dimensions: ReplaySubject<string[]> = new ReplaySubject<string[]>(1);
    private data: { [id: string]: ReplaySubject<number[]> } = {}

    constructor(private http: Http) {
        this.http.get('/api/graph/dimensions')
            .subscribe(res => this.dimensions.next(<string[]>res.json()));
    }

    public getData(dim: string): ReplaySubject<number[]> {
        if (this.data[dim] !== undefined) {
            let rs = new ReplaySubject<number[]>(1);
            this.data[dim] = rs;
            this.http.post('/api/graph/data', { dimension: dim })
                .subscribe(res => rs.next(res.json()));
        }

        return this.data[dim];
    }
}
