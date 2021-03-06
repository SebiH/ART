import { Filter } from './filter';
import { Graph } from '../graph';
import { Point } from '../point';
import { ChartDimension } from '../chart-dimension';
import { Utils } from '../../Utils';

import * as _ from 'lodash';

const DEFAULT_FILTER_COLOUR = "#1E88E5";

export class DetailFilter extends Filter {

    /*
     *    color
     */
    private _color : string = DEFAULT_FILTER_COLOUR;

    public get color() : string {
        return this._color;
    }

    public set color(v : string) {
        if (this._color != v) {
            this._color = v;
            this.propagateUpdates(['color']);
        }
    }


    /*
     *    Use axis color - instead of using a single color for all
     *    indices within the filter, use predefined colors from the
     *    from the selected axis (per category / gradient for metric)
     */
    private _useAxisColor : 'x' | 'y' | 'n' = 'n'; // 'x' / 'y' -> x / y axis, 'n' -> no

    public get useAxisColor() : 'x' | 'y' | 'n' {
        return this._useAxisColor;
    }

    public set useAxisColor(v: 'x' | 'y' | 'n') {
        if (this._useAxisColor != v) {
            this._useAxisColor = v;
            if (v == 'x' || v == 'y') {
                this._color = '#212121';
            }
            this.propagateUpdates(['useAxisColor', 'color']);
        }
    }


    public minX: number;
    public maxX: number;
    public minY: number;
    public maxY: number;


    private delayedRecalculateIndices: Function;

    constructor(id: number) {
        super(id);
        this.delayedRecalculateIndices = _.debounce(this.recalculateIndices, 100);
    }

    public addPathPoint(p: [number, number]): void {
        if (this.origin.getActualXAxis() && this.origin.getActualYAxis()) {
            let x = Math.max(this.origin.getActualXAxis().getMinValue(), Math.min(p[0], this.origin.getActualXAxis().getMaxValue()));
            let y = Math.max(this.origin.getActualYAxis().getMinValue(), Math.min(p[1], this.origin.getActualYAxis().getMaxValue()));

            if (this.path.length == 0) {
                this.minX = x;
                this.maxX = x;
                this.minY = y;
                this.maxY = y;
            } else {
                this.minX = Math.min(x, this.minX);
                this.maxX = Math.max(x, this.maxX);
                this.minY = Math.min(y, this.minY);
                this.maxY = Math.max(y, this.maxY);
            }

            this.path.push([x, y]);
            this.propagateUpdates(['path']);
            this.delayedRecalculateIndices();
        }
    }

    public setPathPoint(index: number, p: [number, number]): void {
        let x = Math.max(this.origin.getActualXAxis().getMinValue(), Math.min(p[0], this.origin.getActualXAxis().getMaxValue()));
        let y = Math.max(this.origin.getActualYAxis().getMinValue(), Math.min(p[1], this.origin.getActualYAxis().getMaxValue()));

        this.path[index] = [x, y];

        this.minX = this.path[0][0];
        this.maxX = this.path[0][0];
        this.minY = this.path[0][1];
        this.maxY = this.path[0][1];

        for (let i = 0; i < this.path.length; i++) {
            this.minX = Math.min(this.path[i][0], this.minX);
            this.maxX = Math.max(this.path[i][0], this.maxX);
            this.minY = Math.min(this.path[i][1], this.minY);
            this.maxY = Math.max(this.path[i][1], this.maxY);
        }

        this.propagateUpdates(['path']);
        this.delayedRecalculateIndices();
    }

    public clearPath(): void {
        this.path = [];
    }


    protected recalculateIndices(): void {
        let dimX = this.origin.getActualXAxis();
        let dimY = this.origin.getActualYAxis();

        let indices: number[] = [];
        let boundingRect = Utils.buildBoundingRect(this.path);

        // crashes edge otherwise??
        if (dimX && dimX.data && dimY && dimY.data) {
            for (let i = 0; i < dimX.data.length; i++) {
                let d = new Point(dimX.data[i].value, dimY.data[i].value);
                if (d.isInPolygonOf(this.path, boundingRect)) {
                    indices.push(i);
                }
            }
        }

        this.selectedDataIndices = indices;
    }


    protected onDimensionChanged(prevDimX: ChartDimension, prevDimY: ChartDimension): void {
        this.isInvalid = true;
    }


    public getColor(): string {
        return this.color;
    }

    public toJson(): any {
        let jFilter = super.toJson();
        jFilter.color = this.color;
        jFilter.useAxisColor = this.useAxisColor;

        if (this._useAxisColor == 'x') {
            if (this.origin.getActualXAxis().isMetric) {
                jFilter.gradient = this.origin.getActualXAxis().gradient;
                jFilter.range = [this.minX, this.maxX];
            } else {
                jFilter.mappings = this.origin.getActualXAxis().mappings;
            }
        } else if (this._useAxisColor == 'y') {
            if (this.origin.getActualYAxis().isMetric) {
                jFilter.gradient = this.origin.getActualYAxis().gradient;
                jFilter.range = [this.minY, this.maxY];
            } else {
                jFilter.mappings = this.origin.getActualYAxis().mappings;
            }
        }

        return jFilter;
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        let filter = new DetailFilter(jFilter.id);
        filter.applyJsonProperties(jFilter, origin);

        if (filter.path.length > 0) {
            filter.minX = _.minBy(filter.path, (v) => v[0])[0];
            filter.maxX = _.maxBy(filter.path, (v) => v[0])[0];
            filter.minY = _.minBy(filter.path, (v) => v[1])[1];
            filter.maxY = _.maxBy(filter.path, (v) => v[1])[1];
        }

        return filter;
    }

    protected applyJsonProperties(jFilter: any, origin: Graph): void {
        super.applyJsonProperties(jFilter, origin);
        this._color = jFilter.color;
        this._useAxisColor = jFilter.useAxisColor;
        this.recalculateIndices();
    }
}
