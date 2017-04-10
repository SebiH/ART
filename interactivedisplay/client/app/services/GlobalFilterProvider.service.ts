import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { Observable } from 'rxjs/Observable';
import { Graph, Filter, ChartDimension } from '../models/index';
import { SocketIO } from './SocketIO.service';
import { FilterProvider } from './FilterProvider.service';
import { GraphProvider } from './GraphProvider.service';
import { Utils } from '../Utils';

import * as _ from 'lodash';

export interface DataHighlight {
    id: number;
    color: string;
    selectedBy: number[];
}

@Injectable()
export class GlobalFilterProvider {
    private globalFilterObserver: ReplaySubject<DataHighlight[]> = new ReplaySubject<DataHighlight[]>(1);
    private globalFilter: DataHighlight[] = [];

    private delayedGlobalFilterUpdate: Function;

    private filters: Filter[] = [];
    private graphs: Graph[] = [];

    constructor(
        private filterProvider: FilterProvider,
        private graphProvider: GraphProvider
        ) {
        this.delayedGlobalFilterUpdate = _.debounce(this.updateGlobalFilter, 100);

        filterProvider.getFilters()
            .subscribe(filters => {
                this.filters = filters;
                this.delayedGlobalFilterUpdate();
            });

        graphProvider.getGraphs()
            .subscribe(graphs => {
                this.graphs = graphs;
            });
    }

    public onUpdate() {
        return this.globalFilterObserver.asObservable();
    }
 

    private updateGlobalFilter(): void {
        // let hasActiveFilter = false;

        // for (let data of this.globalFilter) {
        //     data.color = '#FFFFFF';
        //     data.selectedBy = [];
        // }


        // for (let graph of this.graphs) {

        //     let filters = _.filter(this.filters, (f) => f.origin.id === graph.id);

        //     if (filters.length === 0) {
        //         for (let data of this.globalFilter) {
        //             data.selectedBy.push(graph.id);
        //         }
        //     }

        //     for (let filter of filters) {

        //         hasActiveFilter = hasActiveFilter || filter.indices.length > 0;

        //         for (let index of filter.indices) {
        //             let gfData = this.globalFilter[index];
        //             if (gfData.selectedBy.indexOf(graph.id) < 0) {
        //                 gfData.selectedBy.push(graph.id);
        //             }
        //         }

        //         if (filter.isOverview && filter.origin.isColored) {
        //             if (filter.type === FilterType.Metric) {
        //                 this.calculateMetricGradient(filter);
        //             } else if (filter.type === FilterType.Categorical) {
        //                 for (let index of filter.indices) {
        //                     this.globalFilter[index].color = filter.color;
        //                 }
        //             }

        //         }

        //     }
        // }

        // let syncFilter = [];

        // if (hasActiveFilter) {
        //     for (let data of this.globalFilter) {
        //         syncFilter.push({
        //             id: data.id,
        //             f: data.selectedBy.length < this.graphs.length ? 1 : 0,
        //             c: data.color
        //         });

        //     }
        // } else {
        //     for (let data of this.globalFilter) {
        //         syncFilter.push({
        //             id: data.id,
        //             f: 0,
        //             c: '#FFFFFF'
        //         });
        //     }
        // }

        // this.socketio.sendMessage('globalfilter', { globalfilter: syncFilter });
        // this.globalFilterObserver.next(this.globalFilter);
    }
}
