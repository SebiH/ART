import { Injectable } from '@angular/core';
import { Subject, Observable, Subscription } from 'rxjs/Rx';
import { Graph, Filter, CategoryFilter, MetricFilter, DetailFilter, ChartDimension } from '../models/index';
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

// const InclineGradientNegative = '#F44336';
const InclineGradientNegative = '#FF0000';
const InclineGradientNeutral = '#FFFFFF';
const InclineGradientPositive = '#00FF00';
// const InclineGradientPositive = '#4CAF50';

@Injectable()
export class GlobalFilterProvider {
    private globalFilter: DataHighlight[] = [];

    private delayedGlobalFilterUpdate: Function;

    private filters: Filter[] = [];
    private filterSubscriptions: Subscription[] = [];
    private graphs: Graph[] = [];

    private updateSubject: Subject<any[]> = new Subject<any[]>();

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

    public onUpdate(): Observable<any[]> {
        return this.updateSubject.asObservable();
    }

    private resubscribe(filters: Filter[]) {
        for (let sub of this.filterSubscriptions) {
            sub.unsubscribe();
        }
        this.filterSubscriptions = [];

        for (let filter of filters) {
            let sub = filter.onUpdate
                .filter((changes) => changes.indexOf('selectedDataIndices') >= 0
                    || changes.indexOf('color') >= 0
                    || changes.indexOf('useAxisColor') >= 0)
                .subscribe(() => this.delayedGlobalFilterUpdate());
            this.filterSubscriptions.push(sub);
        }
    }

    private applyFilterColor(filter: Filter): void {
        let detailFilter = filter as DetailFilter;
        if (filter instanceof DetailFilter) {

            switch (filter.useAxisColor) {
                case 'n':
                    for (let index of filter.selectedDataIndices) {
                        this.globalFilter[index].color = filter.color;
                    }
                    break;

                case 'x':
                    this.applyFilterByValue(filter, filter.origin.getActualXAxis(), 'x');
                break;
                case 'y':
                    this.applyFilterByValue(filter, filter.origin.getActualYAxis(), 'y');
                break;
            }
        }
    }

    private applyFilterByValue(filter: DetailFilter, dimension: ChartDimension, axis: 'x' | 'y') {
        if (dimension.isMetric) {
            this.applyFilterGradient(filter, dimension, axis);
        } else {
            for (let index of filter.selectedDataIndices) {
                let datum = dimension.data[index];
                let mapping = _.find(dimension.mappings, (m) => m.value == datum.value);
                if (mapping) {
                    this.globalFilter[index].color = mapping.color;
                }
            }
        }
    }


    private applyFilterGradient(filter: DetailFilter, dimension: ChartDimension, axis: 'x' | 'y') {

        let minValue = axis == 'x' ? filter.minX : filter.minY;
        let maxValue = axis == 'x' ? filter.maxX : filter.maxY;
        let otherFilters = _.filter(this.filters, f => {
            let df = f as DetailFilter;
            return df
                && df != filter
                && df.origin.id == filter.origin.id
                && df.useAxisColor == filter.useAxisColor
                && df.boundDimensions == filter.boundDimensions;
        });

        for (let f of otherFilters) {
            let df = f as DetailFilter;
            minValue = Math.min(minValue, axis == 'x' ? df.minX : df.minY);
            maxValue = Math.max(maxValue, axis == 'x' ? df.maxX : df.maxY);
        }

        for (let index of filter.selectedDataIndices) {
            let data = dimension.data[index].value;
            let relData = (data - minValue) / Math.abs(maxValue - minValue);
            relData = _.clamp(relData, 0, 1);
            this.globalFilter[index].color = Utils.getGradientColor(dimension.gradient, relData);
        }
    }

    private updateGlobalFilter(): void {
        let hasActiveFilter = false;
        let remainingIndices: number[] = [];

        // small hack to initialize all datapoints
        let sampleGraph = _.find(this.graphs, g => g.getActualXAxis() != null || g.getActualYAxis() != null);
        if (sampleGraph == null) {
            return;
        }

        this.globalFilter = [];
        let sampleDim = sampleGraph.getActualXAxis() || sampleGraph.getActualYAxis();
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
                }

                if (filter.boundDimensions == 'y') {
                    selectedY = _.union(selectedY, filter.selectedDataIndices);
                }

                if (filter.boundDimensions == 'xy') {
                    selectedDetail = _.union(selectedDetail, filter.selectedDataIndices);

                    if (graph.isColored) {
                        this.applyFilterColor(filter);
                    }
                }
            }

            if (graph.isColored && filters.length == 0) {
                let dim = graph.getActualYAxis();
                if (dim.isMetric) {
                    for (let f of this.globalFilter) {
                        let data = dim.data[f.id].value;
                        let relData = (data - dim.getActualMinValue()) / Math.abs(dim.getActualMaxValue() - dim.getActualMinValue());
                        relData = _.clamp(relData, 0, 1);
                        f.color = Utils.getGradientColor(dim.gradient, relData);
                    }
                } else {
                    for (let f of this.globalFilter) {
                        let datum = dim.data[f.id];
                        let mapping = _.find(dim.mappings, (m) => m.value == datum.value);
                        if (mapping) {
                            f.color = mapping.color;
                        }
                    }
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



        for (let graph of this.graphs) {
            if (graph.colorIncline) {
                let prevGraph = this.graphProvider.getPrevGraph(graph);
                if (prevGraph) {
                    let leftY = prevGraph.getActualYAxis();
                    let rightY = graph.getActualYAxis();

                    if (leftY && rightY) {
                        let maxIncline = Number.NEGATIVE_INFINITY;
                        let minIncline = Number.POSITIVE_INFINITY;

                        for (let f of this.globalFilter) {
                            if (!f.isFiltered) {
                                let incline = rightY.data[f.id].value - leftY.data[f.id].value;
                                maxIncline = Math.max(maxIncline, incline);
                                minIncline = Math.min(minIncline, incline);
                            }
                        }

                        let range = maxIncline - minIncline;

                        if (Math.abs(range) > 0) {
                            // let mid = minIncline + range / 2;
                            let mid = 0;

                            for (let f of this.globalFilter) {
                                let l = leftY.data[f.id].value;
                                let r = rightY.data[f.id].value;
                                let incline = r - l;

                                if (incline < mid) {
                                    let relativeIncline = (incline - minIncline) / (mid - minIncline);

                                    f.color = Utils.getGradientColor([
                                        { stop: 0, color: InclineGradientNegative },
                                        { stop: 1, color: InclineGradientNeutral },
                                    ], relativeIncline)

                                } else {
                                    let relativeIncline = (incline - mid) / (maxIncline - mid);

                                    f.color = Utils.getGradientColor([
                                        { stop: 0, color: InclineGradientNeutral },
                                        { stop: 1, color: InclineGradientPositive },
                                    ], relativeIncline)
                                }
                            }

                        } else {
                            // all neutral
                            for (let f of this.globalFilter) {
                                f.color = InclineGradientNeutral;
                            }
                        }
                    }
                }
            }

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
        this.updateSubject.next(syncFilter);
    }


    // quick workaround for selecting data indices via admin panel
    public adminFilterHack(index: number): void {
        let syncFilter = [];

        for (let data of this.globalFilter) {
            let isFiltered = data.id != index;
            syncFilter.push({
                id: data.id,
                f: isFiltered ? 1 : 0,
                c: isFiltered ? "#FFFFFF" : "#F44336"
            });
        }

        this.socketio.sendMessage('globalfilter', { globalfilter: syncFilter });
    }

    public adminUpdateGlobalFilter(): void {
        this.updateGlobalFilter();
    }
}
