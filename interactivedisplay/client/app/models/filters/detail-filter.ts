import { Filter } from './filter';
import { Graph } from '../graph';
import { Point } from '../point';
import { ChartDimension } from '../chart-dimension';
import { Utils } from '../../Utils';

import * as _ from 'lodash';

const DEFAULT_FILTER_COLOUR = "#03A9F4";

export class DetailFilter extends Filter {

    private delayedRecalculateIndices: Function;

    constructor(id: number) {
        super(id);
        this.delayedRecalculateIndices = _.debounce(this.recalculateIndices, 100);
    }

    public addPathPoint(p: [number, number]): void {
        let x = Math.max(this.origin.dimX.getMinValue(), Math.min(p[0], this.origin.dimX.getMaxValue()));
        let y = Math.max(this.origin.dimY.getMinValue(), Math.min(p[1], this.origin.dimY.getMaxValue()));

        this.path.push([x, y]);
        this.propagateUpdates(['path']);
        this.delayedRecalculateIndices();
    }

    public clearPath(): void {
        this.path = [];
    }


    protected recalculateIndices(): void {
        let dimX = this.origin.dimX;
        let dimY = this.origin.dimY;

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
        return DEFAULT_FILTER_COLOUR;
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        let filter = new DetailFilter(jFilter.id);
        filter.applyJsonProperties(jFilter, origin);
        return filter;
    }

    protected applyJsonProperties(jFilter: any, origin: Graph): void {
        super.applyJsonProperties(jFilter, origin);
        this.recalculateIndices();
    }
}
