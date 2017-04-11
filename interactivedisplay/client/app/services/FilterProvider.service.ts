import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { Observable } from 'rxjs/Observable';
import { Graph, Filter, DetailFilter, CategoryFilter, MetricFilter } from '../models/index';
import { SocketIO } from './SocketIO.service';
import { GraphProvider } from './GraphProvider.service';
import { Utils } from '../Utils';

import * as _ from 'lodash';

enum FilterType {
    Categorical = 0,
    Metric = 1,
    Detail = 2   
}

@Injectable()
export class FilterProvider {

    private filters: Filter[] = [];
    private filterObserver: ReplaySubject<Filter[]> = new ReplaySubject<Filter[]>(1);
    private idCounter: number = 0;

    private delayedFilterSync: Function;

    constructor(
        private http: Http,
        private socketio: SocketIO,
        private graphProvider: GraphProvider
        ) {
        this.graphProvider.getGraphs()
            .first()
            .subscribe(graphs => { 
                this.initFilters(graphs);
            });


        this.graphProvider.onGraphColorChange()
            .subscribe(graph => this.colorUpdate(graph));

        this.graphProvider.onGraphDeletion()
            .subscribe(graph => this.clearFilters(graph));

        this.delayedFilterSync = _.debounce(this.syncFilters, 0);
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
                        let filter = this.fromJson(rFilter, originGraph);
                        localFilters.push(filter);
                        this.attachListeners(filter);
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


    private fromJson(rFilter: any, graph: Graph): Filter {
        switch (<FilterType>rFilter.type) {
            case FilterType.Detail:
                return DetailFilter.fromJson(rFilter, graph);
            case FilterType.Metric:
                return MetricFilter.fromJson(rFilter, graph);
            case FilterType.Categorical:
                return CategoryFilter.fromJson(rFilter, graph);
            default:
                console.error('invalid filter type ' + rFilter.type);
                return null;
        }
    }

    private clearFilters(graph: Graph): void {
        let removeQueue = _.filter(this.filters, (f) => f.origin.id == graph.id);

        for (let filter of removeQueue) {
            this.removeFilter(filter);
        }
    }

    private getJson(filter: Filter): any {
        let jFilter = filter.toJson();

        if (filter instanceof CategoryFilter) {
            jFilter.type = FilterType.Categorical;
        }
        if (filter instanceof MetricFilter) {
            jFilter.type = FilterType.Metric;
        }
        if (filter instanceof DetailFilter) {
            jFilter.type = FilterType.Detail;
        }

        return jFilter;
    }

    private attachListeners(filter: Filter): void {
        filter.onUpdate
            .takeWhile(() => this.filters.indexOf(filter) >= 0)
            .filter(changes => changes.indexOf('isInvalid') >= 0)
            .subscribe(() => this.removeFilter(filter));

        filter.onUpdate
            .takeWhile(() => this.filters.indexOf(filter) >= 0)
            .filter((changes) => changes.indexOf('selectedDataIndices') < 0 && changes.indexOf('isInvalid') < 0)
            .subscribe(() => this.syncFilter(filter));
    }

    private setupFilter(filter: Filter, graph: Graph): void {
        filter.origin = graph;
        this.filters.push(filter);
        this.filterObserver.next(this.filters);
        this.socketio.sendMessage('+filter', this.getJson(filter));
        this.attachListeners(filter);
    }

    private syncFilter(filter: Filter): void {
        this.filterUpdateQueue[filter.id] = this.getJson(filter);
        this.delayedFilterSync();
    }

    private colorUpdate(graph: Graph): void {
        let autoFilters = _.filter(this.filters, f => !f.isUserGenerated);

        for (let filter of autoFilters) {
            this.removeFilter(filter);
        }

        if (graph !== null) {
            let filters = _.filter(this.filters, f => f.origin.id == graph.id);

            if (filters.length == 0) {
                let dimension = graph.isFlipped ? graph.dimY : graph.dimX;
                let axis: 'x' | 'y' = graph.isFlipped ? 'y' : 'x';
                if (dimension.isMetric) {
                    let filter = this.createMetricFilter(graph);
                    filter.isUserGenerated = false;
                    filter.boundDimensions = axis;
                    filter.gradient = dimension.gradient;
                    filter.range = dimension.domain;
                } else {
                    for (let map of dimension.mappings) {
                        let filter = this.createCategoryFilter(graph);
                        filter.boundDimensions = axis;
                        filter.isUserGenerated = false;
                        filter.category = map.value;
                        filter.color = map.color;
                    }
                }
            }
        }
    }



    public createDetailFilter(origin): DetailFilter {
        let filter = new DetailFilter(this.idCounter++);
        this.setupFilter(filter, origin);
        return filter;
    }

    public createCategoryFilter(origin): CategoryFilter {
        let filter = new CategoryFilter(this.idCounter++);
        this.setupFilter(filter, origin);
        return filter;
    }

    public createMetricFilter(origin): MetricFilter {
        let filter = new MetricFilter(this.idCounter++);
        this.setupFilter(filter, origin);
        return filter;
    }

    public getFilters() {
        return this.filterObserver.asObservable();
    }

    public removeFilter(filter: Filter): void {
        this.socketio.sendMessage('-filter', filter.id);
        _.pull(this.filters, filter);
        filter.destroy();
        this.filterObserver.next(this.filters);
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

}
