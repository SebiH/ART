import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { Graph, Filter } from '../models/index';
import { SocketIO } from './SocketIO.service';
import { GraphProvider } from './GraphProvider.service';

import * as _ from 'lodash';

@Injectable()
export class FilterProvider {
    private filters: Filter[] = [];
    private idCounter: number = 0;
    private filterObserver: ReplaySubject<Filter[]> = new ReplaySubject<Filter[]>(1);

    private delayedFilterSync: Function;

    constructor(
        private graphProvider: GraphProvider,
        private socketio: SocketIO,
        private http: Http) {

        this.graphProvider.getGraphs()
            .first()
            .subscribe(graphs => this.initFilters(graphs));

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
    }


    public updateFilter(filter: Filter): void {
        // TODO: recalculate filter selected indices!
        this.filterUpdateQueue[filter.id] = filter.toJson();
        this.delayedFilterSync();
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
    }
}
