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
                        let overviewDim = this.graphDataProvider.tryGetDimension(filter.origin.isFlipped ? filter.origin.dimY : filter.origin.dimX);
                        if (overviewDim && overviewDim.gradient) {
                            let gradient = overviewDim.gradient;
                            // determine gradient position for each data value
                            for (let index of filter.indices) {
                                let gfData = this.globalFilter[index];
                                let val = overviewDim.data[index];

                                for (let i = 0; i < gradient.length - 1; i++) {
                                    let currStop = gradient[i];
                                    let nextStop = gradient[i + 1];

                                    if (currStop.stop <= val && val <= nextStop.stop) {
                                        let range = Math.abs(currStop.stop - nextStop.stop);
                                        gfData.color = this.lerpColor(currStop.color, nextStop.color, (val - currStop.stop) / range);
                                        break;
                                    }
                                }
                            }
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
                    f: data.selectedBy.length < this.graphs.length,
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

        this.socketio.sendMessage('globalfilter', { globalfilter: syncFilter });
    }


    private lerpColor(col1: string, col2: string, weight: number): string {
        let rgb1 = this.hexToRgb(col1);
        let rgb2 = this.hexToRgb(col2);

        let hsv1 = this.rgb2hsv(rgb1.r, rgb1.g, rgb1.b);
        let hsv2 = this.rgb2hsv(rgb2.r, rgb2.g, rgb2.b);

        let resultHsv = {
            h: this.lerp(hsv1.h, hsv2.h, weight),
            s: this.lerp(hsv1.s, hsv2.s, weight),
            v: this.lerp(hsv1.v, hsv2.v, weight),
        };
        let resultRgb = this.hsv2rgb(resultHsv.h, resultHsv.s, resultHsv.v);
        return this.rgbToHex(resultRgb.r, resultRgb.g, resultRgb.b);
    }

    private lerp(val1: number, val2: number, weight: number): number {
        return val1 + weight * (val2 - val1);
    }


    // adapted from http://stackoverflow.com/a/5624139/4090817
    private rgbToHex(r, g, b): string {
        return "#" + this.componentToHex(r) + this.componentToHex(g) + this.componentToHex(b);
    }

    // adapted from http://stackoverflow.com/a/5624139/4090817
    private componentToHex(c): string {
        var hex = c.toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    }

    // adapted from http://stackoverflow.com/a/5624139/4090817
    private hexToRgb(hex) {
        // Expand shorthand form (e.g. "03F") to full form (e.g. "0033FF")
        var shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
        hex = hex.replace(shorthandRegex, function(m, r, g, b) {
            return r + r + g + g + b + b;
        });

        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    }



    // adapted from: http://stackoverflow.com/a/8023734/4090817
    private rgb2hsv(r255, g255, b255) {
        let rr, gg, bb;
        let r = r255 / 255;
        let g = g255 / 255;
        let b = b255 / 255;
        let v = Math.max(r, g, b);
        let diff = v - Math.min(r, g, b);

        let h = 0;
        let s = 0;

        if (diff !== 0) {
            s = diff / v;
            rr = this.diffc(v, r, diff);
            gg = this.diffc(v, g, diff);
            bb = this.diffc(v, b, diff);

            if (r === v) {
                h = bb - gg;
            } else if (g === v) {
                h = (1 / 3) + rr - bb;
            } else if (b === v) {
                h = (2 / 3) + gg - rr;
            }

            if (h < 0) {
                h += 1;
            } else if (h > 1) {
                h -= 1;
            }
        }

        return {
            h: Math.round(h * 360),
            s: s,
            v: v
        };
    }

    private diffc(v, c, diff) {
        return (v - c) / 6 / diff + 1 / 2;
    };


    // adapted from: https://github.com/tmpvar/hsv2rgb
    private set(r, g, b, out) {
        out[0] = Math.round(r * 255);
        out[1] = Math.round(g * 255);
        out[2] = Math.round(b * 255);
    }

    // adapted from: https://github.com/tmpvar/hsv2rgb
    private hsv2rgb(h, s, v) {
        let out = [0, 0, 0];
        h = h % 360;
        s = _.clamp(s, 0, 1);
        v = _.clamp(v, 0, 1);

        // Grey
        if (!s) {
            out[0] = out[1] = out[2] = Math.ceil(v * 255);
        } else {
            var b = ((1 - s) * v);
            var vb = v - b;
            var hm = h % 60;
            switch((h/60)|0) {
                case 0: this.set(v, vb * h / 60 + b, b, out); break;
                case 1: this.set(vb * (60 - hm) / 60 + b, v, b, out); break;
                case 2: this.set(b, v, vb * hm / 60 + b, out); break;
                case 3: this.set(b, vb * (60 - hm) / 60 + b, v, out); break;
                case 4: this.set(vb * hm / 60 + b, b, v, out); break;
                case 5: this.set(v, b, vb * (60 - hm) / 60 + b, out); break;
            }
        }

        return {
            r: out[0],
            g: out[1],
            b: out[2]
        };
    }
}
