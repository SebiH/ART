import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { Graph, Filter, FilterType } from '../models/index';
import { SocketIO } from './SocketIO.service';
import { GraphProvider } from './GraphProvider.service';
import { GraphDataProvider } from './GraphDataProvider.service';

import * as _ from 'lodash';

interface DataHighlight {
    id: number;
    color: string;
    selectedBy: number[];
}

@Injectable()
export class FilterProvider {
    private filters: Filter[] = [];
    private idCounter: number = 0;
    private filterObserver: ReplaySubject<Filter[]> = new ReplaySubject<Filter[]>(1);

    private graphs: Graph[] = [];
    private globalFilter: DataHighlight[] = [];

    private delayedFilterSync: Function;
    private delayedGlobalFilterUpdate: Function;

    constructor(
        private graphProvider: GraphProvider,
        private graphDataProvider: GraphDataProvider,
        private socketio: SocketIO,
        private http: Http) {

        this.graphProvider.getGraphs()
            .first()
            .subscribe(graphs => { 
                this.graphs = graphs;
                this.initFilters(graphs);
            });

        this.graphProvider.onGraphDeletion()
            .subscribe(graph => this.clearFilters(graph));

        this.graphDataProvider.onDataCount()
            .filter(max => this.globalFilter.length != max)
            .subscribe(max => this.initGlobalFilter(max));

        this.delayedFilterSync = _.debounce(this.syncFilters, 0);
        this.delayedGlobalFilterUpdate = _.debounce(this.updateGlobalFilter, 100);
    }

    private initFilters(graphs: Graph[]) {
        this.http.get('/api/filter/list')
            .subscribe(response => {
                let remoteFilters = response.json().filters;
                let localFilters: Filter[] = [];

                for (let rFilter of remoteFilters) {
                    this.idCounter = Math.max(this.idCounter, rFilter.id + 1);

                    let originGraph = _.find(graphs, g => g.id === rFilter.origin);
                    if (originGraph) {
                        let filter = Filter.fromJson(rFilter, originGraph);
                        localFilters.push(filter);
                    } else {
                        console.warn('Could not find origin graph ' + rFilter.origin + ' for filter ' + rFilter.id)
                    }
                }

                this.filters = localFilters;
                this.filterObserver.next(this.filters);

                // for debugging
                window['filters'] = this.filters;
            });
    }


    private filterUpdateQueue: { [id: number]: any } = {};

    private syncFilters(): void {
        for (let id of _.keys(this.filterUpdateQueue)) {
            if (_.find(this.filters, f => f.id == +id) == null) {
                delete this.filterUpdateQueue[+id];
            }
        }

        let filters = _.values(this.filterUpdateQueue);

        if (filters.length > 0) {
            this.socketio.sendMessage('filter', {
                filters: filters
            });
            this.filterUpdateQueue = {};
        }
    }

    private clearFilters(graph: Graph): void {
        let removeQueue = _.filter(this.filters, (f) => f.origin.id == graph.id);

        for (let filter of removeQueue) {
            this.removeFilter(filter);
        }

        this.delayedGlobalFilterUpdate();
    }

    public getDataAttributes(index: number) {
        return this.globalFilter[index];
    }

    public updateFilter(filter: Filter): void {
        let overviewDim = this.graphDataProvider.tryGetDimension(filter.origin.isFlipped ? filter.origin.dimY : filter.origin.dimX);

        if (overviewDim) {
            filter.indices = [];

            switch (filter.type) {
                case FilterType.Detail:
                    // TODO: compare paths <-> points
                    break;

                case FilterType.Categorical:
                    for (let i = 0; i < overviewDim.data.length; i++) {
                        if (overviewDim.data[i] === filter.category) {
                            filter.indices.push(i);
                        }
                    }
                    break;

                case FilterType.Metric:
                    for (let i = 0; i < overviewDim.data.length; i++) {
                        let data = overviewDim.data[i];
                        let minRange = Math.min(filter.range[0], filter.range[1]);
                        let maxRange = Math.max(filter.range[0], filter.range[1]);

                        if (minRange <= data && data <= maxRange) {
                            filter.indices.push(i);
                        }
                    }
                    break;
            }
        }

        this.filterUpdateQueue[filter.id] = filter.toJson();
        this.delayedFilterSync();
        this.delayedGlobalFilterUpdate();
    }

    public createFilter(origin: Graph): Filter {
        let filter = new Filter(this.idCounter++);
        filter.origin = origin;

        this.filters.push(filter);
        this.filterObserver.next(this.filters);
        this.socketio.sendMessage('+filter', filter.toJson());

        return filter;
    }

    public getFilters() {
        return this.filterObserver.asObservable();
    }

    public removeFilter(filter: Filter): void {
        this.socketio.sendMessage('-filter', filter.id);
        _.pull(this.filters, filter);
        this.filterObserver.next(this.filters);
        this.delayedGlobalFilterUpdate();
    }



    private initGlobalFilter(max: number): void {
        this.globalFilter = [];
        for (let i = 0; i < max; i++) {
            this.globalFilter[i] = {
                id: i,
                selectedBy: [],
                color: '#FFFFFF'
            }
        }

        this.delayedGlobalFilterUpdate();

        window['globalFilter'] = this.globalFilter;
    }

    private updateGlobalFilter(): void {
        let hasActiveFilter = false;

        for (let data of this.globalFilter) {
            data.color = '#FFFFFF';
            data.selectedBy = [];
        }


        for (let graph of this.graphs) {

            let filters = _.filter(this.filters, (f) => f.origin.id === graph.id);

            if (filters.length === 0) {
                for (let data of this.globalFilter) {
                    data.selectedBy.push(graph.id);
                }
            }

            for (let filter of filters) {

                hasActiveFilter = hasActiveFilter || filter.indices.length > 0;

                for (let index of filter.indices) {
                    let gfData = this.globalFilter[index];
                    if (gfData.selectedBy.indexOf(graph.id) < 0) {
                        gfData.selectedBy.push(graph.id);
                    }
                }

                if (filter.isOverview && filter.origin.isColored) {
                    if (filter.type === FilterType.Metric) {
                        for (let index of filter.indices) {
                            // TODO: determine gradient position
                        }
                    } else if (filter.type === FilterType.Categorical) {
                        for (let index of filter.indices) {
                            this.globalFilter[index].color = filter.color;
                        }
                    }

                }

            }
        }

        let syncFilter = [];

        if (hasActiveFilter) {
            for (let data of this.globalFilter) {
                syncFilter.push({
                    id: data.id,
                    f: data.selectedBy.length >= this.graphs.length,
                    c: data.color
                });

            }
        } else {
            for (let data of this.globalFilter) {
                syncFilter.push({
                    id: data.id,
                    f: false,
                    c: '#FFFFFF'
                });
            }
        }

        this.socketio.sendMessage('globalfilter', syncFilter);
    }
}
