import { Injectable } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { Graph, Filter, CategoryFilter, MetricFilter, ChartDimension } from '../models/index';
import { SocketIO } from './SocketIO.service';
import { FilterProvider } from './FilterProvider.service';
import { GraphProvider } from './GraphProvider.service';
import { Utils } from '../Utils';

import * as _ from 'lodash';

interface DataHighlight {
    id: number;
    color: string;
    isFiltered: boolean;
}

@Injectable()
export class GlobalFilterProvider {
    private globalFilter: DataHighlight[] = [];

    private delayedGlobalFilterUpdate: Function;

    private filters: Filter[] = [];
    private filterSubscriptions: Subscription[] = [];
    private graphs: Graph[] = [];

    constructor(
        private filterProvider: FilterProvider,
        private graphProvider: GraphProvider,
        private socketio: SocketIO
        ) {
        this.delayedGlobalFilterUpdate = _.debounce(this.updateGlobalFilter, 100);

        filterProvider.getFilters()
            .subscribe(filters => {
                this.resubscribe(filters);
                this.filters = filters;
                this.delayedGlobalFilterUpdate();
            });

        graphProvider.getGraphs()
            .subscribe(graphs => {
                this.graphs = graphs;
            });

        graphProvider.onGraphColorChange()
            .subscribe(() => {
                this.delayedGlobalFilterUpdate();
            });
    }


    private resubscribe(filters: Filter[]) {
        for (let sub of this.filterSubscriptions) {
            sub.unsubscribe();
        }
        this.filterSubscriptions = [];

        for (let filter of filters) {
            let sub = filter.onUpdate
                .filter((changes) => changes.indexOf('selectedDataIndices') >= 0)
                .subscribe(() => this.delayedGlobalFilterUpdate());
            this.filterSubscriptions.push(sub);
        }
    }

    private applyFilterColor(filter: Filter): void {
        let categoryFilter = filter as CategoryFilter;
        if (categoryFilter) {
            for (let index of filter.selectedDataIndices) {
                this.globalFilter[index].color = categoryFilter.color;
            }
        }


        let metricFilter= filter as MetricFilter;
        if (metricFilter) {
            if (filter.boundDimensions == 'x') {
                this.applyFilterGradient(metricFilter, metricFilter.origin.dimX);
            } else {
                this.applyFilterGradient(metricFilter, metricFilter.origin.dimY);
            }
        }
    }


    private applyFilterGradient(filter: MetricFilter, dimension: ChartDimension) {

        let minValue = filter.range.min;
        let maxValue = filter.range.max;
        let otherFilters = _.filter(this.filters, f => {
            let mf = f as MetricFilter;
            return mf &&
                mf != filter &&
                mf.origin.id == filter.origin.id &&
                mf.boundDimensions == filter.boundDimensions;
        });

        for (let f of otherFilters) {
            minValue = Math.min(minValue, (f as MetricFilter).range.min);
            maxValue = Math.max(maxValue, (f as MetricFilter).range.max);
        }

        for (let index of filter.selectedDataIndices) {
            let data = dimension.data[index].value;
            let relData = (data - minValue) / Math.abs(maxValue - minValue);
            relData = _.clamp(relData, 0, 1);
            this.globalFilter[index].color = Utils.getGradientColor(filter.gradient, relData);
        }
    }

    private updateGlobalFilter(): void {
        let hasActiveFilter = false;
        let remainingIndices: number[] = [];

        // small hack to initialize all datapoints
        let sampleGraph = _.find(this.graphs, g => g.dimX != null || g.dimY != null);
        if (sampleGraph == null) {
            return;
        }

        this.globalFilter = [];
        let sampleDim = sampleGraph.dimX || sampleGraph.dimY;
        for (let i = 0; i < sampleDim.data.length; i++) {
            this.globalFilter.push({
                id: i,
                color: '#FFFFFF',
                isFiltered: true
            });
            remainingIndices.push(i);
        }

        for (let graph of this.graphs) {
            let filters = _.filter(this.filters, f => f.origin.id === graph.id);

            let selectedX: number[] = null;
            let selectedY: number[] = null;
            let selectedDetail: number[] = null;

            for (let filter of filters) {
                if (filter.boundDimensions == 'x') {
                    selectedX = _.union(selectedX, filter.selectedDataIndices);

                    if (graph.useColorX) {
                        this.applyFilterColor(filter);
                    }
                }

                if (filter.boundDimensions == 'y') {
                    selectedY = _.union(selectedY, filter.selectedDataIndices);

                    if (graph.useColorY) {
                        this.applyFilterColor(filter);
                    }
                }

                if (filter.boundDimensions == 'xy') {
                    selectedDetail = _.union(selectedDetail, filter.selectedDataIndices);
                }
            }

            let selectedXY: number[] = [];

            if (!(selectedX) || selectedX.length == 0) {
                selectedXY = selectedY;
            } else if (!(selectedY) || selectedY.length == 0) {
                selectedXY = selectedX;
            } else {
                selectedXY = _.intersection(selectedX, selectedY);
            }

            let selectedTotal: number[] = _.union(selectedDetail, selectedXY);
            if (selectedTotal.length > 0) {
                remainingIndices = _.intersection(selectedTotal, remainingIndices);
            }
        }

        for (let index of remainingIndices) {
            this.globalFilter[index].isFiltered = false;
        }




        let syncFilter = [];

        for (let data of this.globalFilter) {
            let isFiltered = remainingIndices.length > 0 && data.isFiltered;
            syncFilter.push({
                id: data.id,
                f: isFiltered ? 1 : 0,
                c: data.color
            });
        }

        this.socketio.sendMessage('globalfilter', { globalfilter: syncFilter });
    }
}
