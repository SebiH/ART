import { Filter } from './filter';
import { Graph } from '../graph';
import { ChartDimension, GradientStop } from '../chart-dimension';

interface Range {
    readonly min: number,
    readonly max: number
}

export class MetricFilter extends Filter {

    /*
     *    gradient
     */
    private _gradient : GradientStop[];
    public get gradient() : GradientStop[] {
        return this._gradient;
    }
    public set gradient(v : GradientStop[]) {
        if (this._gradient != v) {
            this._gradient = v;
            this.propagateUpdates(['gradient']);
        }
    }

    /*
     *    range
     */
    private _range: Range = { min: 0, max: 0 };
    public get range(): Range {
        return this._range;
    }
    public set range(v: Range) {
        if (v.min != this._range.min || v.max != this._range.max) {
            this._range = v;
            this.propagateUpdates(['range']);
            this.generatePath();
        }
    }

    public setRange(v: [number, number]): void {
        let sortedRange: Range = {
            min: Math.min(v[0], v[1]),
            max: Math.max(v[0], v[1])
        };
        this.range = sortedRange;
    }

    private generatePath(): void {
        let left, right, top, bottom;

        if (this.boundDimensions == 'x') {
            left = this.range.min;
            right = this.range.max;
            top = this.origin.dimY.getMaxValue();
            bottom = this.origin.dimY.getMinValue();
        } else {
            left = this.origin.dimX.getMinValue();
            right = this.origin.dimX.getMaxValue();
            top = this.range.max;
            bottom = this.range.min;
        }

        this.path = [[left, top], [left, bottom], [right, bottom], [right, top]];
    }


    public onDimensionChanged(prevDimX: ChartDimension, prevDimY: ChartDimension): void {
        if (this.origin.dimX !== prevDimX && this.boundDimensions == 'x') {
            this.isInvalid = true;
        } else if (this.origin.dimY !== prevDimY && this.boundDimensions == 'y') {
            this.isInvalid = true;
        } else {
            this.generatePath();
        }
    }


    public getColor(): string {
        return this._gradient[this._gradient.length - 1].color;
    }

    public toJson(): any {
        let jFilter = super.toJson();
        jFilter.range = [ this._range.min, this._range.max ];
        jFilter.gradient = this.gradient;
        
        return jFilter;
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        let filter = new MetricFilter(jFilter.id);
        filter.applyJsonProperties(jFilter, origin);
        return filter;
    }

    protected applyJsonProperties(jFilter: any, origin: Graph): void {
        super.applyJsonProperties(jFilter, origin);
        this._range = { min: jFilter.range[0], max: jFilter.range[1] };
        this._gradient = jFilter.gradient;
    }
}
