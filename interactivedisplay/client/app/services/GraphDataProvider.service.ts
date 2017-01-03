import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import * as _ from 'lodash';

@Injectable()
export class GraphDataProvider {

    constructor(private http: Http) {
    }

    public getDimensions(): string[] {
        return ['dims'];
    }

    public getData(dim: string): number[] {
        return [1];
    }
}
