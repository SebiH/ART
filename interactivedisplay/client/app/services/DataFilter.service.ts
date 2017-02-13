import { Injectable } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { GraphProvider } from './GraphProvider.service';
import { Graph } from '../models/index';

import * as _ from 'lodash';

@Injectable()
export class DataFilter {

    private filterSubject: ReplaySubject<number[]> = new ReplaySubject<number[]>(1);
    private graphSubscriptions: Subscription[] = [];
    private graphs: Graph[] = [];

    private currentFilter: number[] = null;

    constructor(private graphProvider: GraphProvider) {
        // null => no filter
        this.filterSubject.next(null);

        this.graphProvider.getGraphs()
            .subscribe(graphs => {
                this.graphs = graphs;
                this.registerGraphListeners();
                this.rebuildFilter();
            });
    }

    public getFilter(): ReplaySubject<number[]> {
        return this.filterSubject;
    }

    public getCurrentFilter(): number[] {
        return this.currentFilter;
    }

    private registerGraphListeners(): void {
        for (let sub of this.graphSubscriptions) {
            sub.unsubscribe();
        }

        for (let graph of this.graphs) {
            graph.onDataUpdate
                .filter(g => g.changes.indexOf('selectedDataIndices') > -1)
                .subscribe(g => this.rebuildFilter());
        }
    }

    private rebuildFilter(): void {
        let graphFilters: number[][] = [];

        for (let graph of this.graphs) {
            if (graph.selectedDataIndices.length > 0) {
                graphFilters.push(graph.selectedDataIndices);
            }
        }

        let useFilter = graphFilters.length > 0;

        if (useFilter) {
            this.currentFilter = _.intersection.apply(_, graphFilters);
        } else {
            this.currentFilter = null;
        }

        this.filterSubject.next(this.currentFilter);
    }
}
