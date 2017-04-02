import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { Observable } from 'rxjs/Observable';
import { Graph, Filter, FilterType, Point, ChartDimension } from '../models/index';
import { SocketIO } from './SocketIO.service';
import { GraphProvider } from './GraphProvider.service';
import { GraphDataProvider } from './GraphDataProvider.service';
import { Utils } from '../Utils';

import * as _ from 'lodash';

export interface DataHighlight {
    id: number;
    color: string;
    selectedBy: number[];
}

@Injectable()
export class FilterProvider {
    private filters: Filter[] = [];
    private idCounter: number = 0;
    private filterObserver: ReplaySubject<Filter[]> = new ReplaySubject<Filter[]>(1);
    private globalFilterObserver: ReplaySubject<DataHighlight[]> = new ReplaySubject<DataHighlight[]>(1);

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
                        this.updateFilter(filter);
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
        Observable
            .zip(
                this.graphDataProvider.getData(filter.origin.dimX),
                this.graphDataProvider.getData(filter.origin.dimY),
                (x, y) => { return { x: x, y: y }; })
            .first()
            .subscribe((dim) => this.doUpdateFilter(filter, dim.x, dim.y));
    }

    private doUpdateFilter(filter: Filter, dimX: ChartDimension, dimY: ChartDimension) {
        let overviewDim = filter.origin.isFlipped ? dimY : dimX;

        filter.indices = [];

        switch (filter.type) {
            case FilterType.Detail:
                let boundingRect = Utils.buildBoundingRect(filter.path);
                for (let i = 0; i < dimX.data.length; i++) {

                    let p: Point;
                    if (filter.origin.isFlipped) {
                        p = new Point(dimY.data[i], dimX.data[i]);
                    } else {
                        p = new Point(dimX.data[i], dimY.data[i]);
                    }
                    if (p.isInPolygonOf(filter.path, boundingRect)) {
                        filter.indices.push(i);
                    }
                }
                break;

            case FilterType.Categorical:
                for (let i = 0; i < overviewDim.data.length; i++) {
                    if (overviewDim.data[i] === filter.category) {
                        filter.indices.push(i);
                    }

                    let paddingScale = 1.1;
                    let rangeX = dimX.domain.max - dimX.domain.min;
                    let rangeY = dimY.domain.max - dimY.domain.min;
                    let minX = dimX.domain.min - rangeX * 0.1;
                    let maxX = dimX.domain.max + rangeX * 0.1;
                    let minY = dimY.domain.min - rangeY * 0.1;
                    let maxY = dimY.domain.max + rangeY * 0.1;

                    if (filter.origin.isFlipped) {
                        filter.path = [
                            [minX, filter.category - 0.5],
                            [maxX, filter.category - 0.5],
                            [maxX, filter.category + 0.5],
                            [minX, filter.category + 0.5],
                        ];
                    } else {
                        filter.path = [
                            [filter.category - 0.5, minY],
                            [filter.category - 0.5, maxY],
                            [filter.category + 0.5, maxY],
                            [filter.category + 0.5, minY],
                        ];
                    }
                }
                break;

            case FilterType.Metric:
                for (let i = 0; i < overviewDim.data.length; i++) {
                    let data = overviewDim.data[i];
                    let minRange = Math.min(filter.range[0], filter.range[1]);
                    let maxRange = Math.max(filter.range[0], filter.range[1]);
                    filter.gradient = overviewDim.gradient;

                    if (minRange <= data && data <= maxRange) {
                        filter.indices.push(i);
                    }

                    if (filter.origin.isFlipped) {
                        // add more paths for better gradient resolution
                        filter.path = [
                            [dimX.domain.min, minRange],
                            [this.half(dimX.domain.min, dimX.domain.max), minRange],
                            [dimX.domain.max, minRange],
                            [dimX.domain.max, this.half(minRange, maxRange)],
                            [dimX.domain.max, maxRange],
                            [this.half(dimX.domain.min, dimX.domain.max), maxRange],
                            [dimX.domain.min, maxRange],
                            [dimX.domain.min, this.half(minRange, maxRange)],
                            [this.half(dimX.domain.min, dimX.domain.max), this.half(minRange, maxRange)],
                            [dimX.domain.min, this.half(minRange, maxRange)],
                        ];
                    } else {
                        // add more paths for better gradient resolution
                        filter.path = [
                            [minRange, dimY.domain.min],
                            [minRange, this.half(dimY.domain.min, dimY.domain.max)],
                            [minRange, dimY.domain.max],
                            [this.half(minRange, maxRange), dimY.domain.max],
                            [maxRange, dimY.domain.max],
                            [maxRange, this.half(dimY.domain.min, dimY.domain.max)],
                            [maxRange, dimY.domain.min],
                            [this.half(minRange, maxRange), dimY.domain.min],
                            [this.half(minRange, maxRange), this.half(dimY.domain.min, dimY.domain.max)],
                            [this.half(minRange, maxRange), dimY.domain.min],
                        ];
                    }
                }
                break;
        }

        this.filterUpdateQueue[filter.id] = filter.toJson();
        this.delayedFilterSync();
        this.delayedGlobalFilterUpdate();

    }

    private half(min: number, max: number): number {
        return min + ((max - min) / 2);
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

    public removeFilters(graph: Graph): void {
        let filtersToRemove: Filter[] = [];
        for (let filter of this.filters) {
            if (filter.origin.id == graph.id) {
                filtersToRemove.push(filter);
            }
        }

        for (let filter of filtersToRemove) {
            this.removeFilter(filter);
        }
    }

    public triggerGlobalFilterUpdate(): void {
        this.delayedGlobalFilterUpdate();
    }

    public globalFilterUpdate() {
        return this.globalFilterObserver.asObservable();
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
                        this.calculateMetricGradient(filter);
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
                    f: data.selectedBy.length < this.graphs.length ? 1 : 0,
                    c: data.color
                });

            }
        } else {
            for (let data of this.globalFilter) {
                syncFilter.push({
                    id: data.id,
                    f: 0,
                    c: '#FFFFFF'
                });
            }
        }

        this.socketio.sendMessage('globalfilter', { globalfilter: syncFilter });
        this.globalFilterObserver.next(this.globalFilter);
    }


    private calculateMetricGradient(filter: Filter) {
        this.graphDataProvider
            .getData(filter.origin.isFlipped ? filter.origin.dimY : filter.origin.dimX)
            .first()
            .subscribe((overviewDim) => {
                if (overviewDim && overviewDim.gradient) {

                    let minValue = filter.range[0];
                    let maxValue = filter.range[1];

                    for (let f of this.filters) {
                        if (f.origin.id == filter.origin.id && f.isOverview && f.type == FilterType.Metric) {
                            minValue = Math.min(minValue, Math.min(f.range[0], f.range[1]));
                            maxValue = Math.max(maxValue, Math.max(f.range[0], f.range[1]));
                        }
                    }

                    minValue = Math.max(overviewDim.domain.min, minValue);
                    maxValue = Math.min(overviewDim.domain.max, maxValue);

                    let gradient = overviewDim.gradient;
                    // determine gradient position for each data value
                    for (let index of filter.indices) {
                        let gfData = this.globalFilter[index];
                        let val = (overviewDim.data[index] - minValue) / Math.abs(maxValue - minValue);

                        if (val < 0 || val > 1) {
                            // TODO: should not happen?
                            console.warn('Value not inside gradient: ' + val);
                            val = _.clamp(val, 0, 1);
                        } 

                        gfData.color = Utils.getGradientColor(gradient, val);
                    }
                }
            });
    }

}
