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
            let dim = this.boundDimensions == 'x' ? this.origin.getActualXAxis() : this.origin.getActualYAxis();
            let min = Math.max(v.min, dim.getMinValue());
            let max = Math.min(v.max, dim.getMaxValue());

            this._range = {min: min, max: max};
            this.propagateUpdates(['range']);
            this.generatePath();
            this.recalculateIndices();
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
            top = this.origin.getActualYAxis().getMaxValue();
            bottom = this.origin.getActualYAxis().getMinValue();
        } else {
            left = this.origin.getActualXAxis().getMinValue();
            right = this.origin.getActualXAxis().getMaxValue();
            top = this.range.max;
            bottom = this.range.min;
        }

        this.path = [[left, top], [left, bottom], [right, bottom], [right, top]];
    }


    protected recalculateIndices(): void {
        let dim = this.boundDimensions == 'x' ? this.origin.getActualXAxis() : this.origin.getActualYAxis();

        let indices: number[] = [];
        for (let i = 0; i < dim.data.length; i++) {
            let d = dim.data[i].value;
            if (this.range.min <= d && d <= this.range.max) {
                indices.push(i);
            }
        }

        this.selectedDataIndices = indices;
    }

    protected onDimensionChanged(prevDimX: ChartDimension, prevDimY: ChartDimension): void {
        if (this.origin.getActualXAxis() !== prevDimX && this.boundDimensions == 'x') {
            this.isInvalid = true;
        } else if (this.origin.getActualYAxis() !== prevDimY && this.boundDimensions == 'y') {
            this.isInvalid = true;
        } else {
            this.generatePath();
        }
    }


    public getColor(): string {
        if (this._gradient) {
            return this._gradient[this._gradient.length - 1].color;
        } else {
            return "#FFFFFF";
        }
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
        this.recalculateIndices();
    }
}
