import { Filter } from './filter';
import { Graph } from '../graph';
import { ChartDimension } from '../chart-dimension';

export class CategoryFilter extends Filter {

    /*
     *    color
     */
    private _color : string;
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
     *    category
     */
    protected _category : number;
    public get category() : number {
        return this._category;
    }
    public set category(v : number) {
        if (this._category !== v) {
            this._category = v;
            this.propagateUpdates(['category']);
            this.generatePath();
            this.recalculateIndices();
        }
    }


    private generatePath(): void {
        let left, right, top, bottom;

        if (this.boundDimensions == 'x') {
            left = this.category - 0.5;
            right = this.category + 0.5;
            top = this.origin.getActualYAxis().getMaxValue();
            bottom = this.origin.getActualYAxis().getMinValue();
        } else {
            left = this.origin.getActualXAxis().getMinValue();
            right = this.origin.getActualXAxis().getMaxValue();
            top = this.category - 0.5;
            bottom = this.category + 0.5;
        }

        this.path = [[left, top], [left, bottom], [right, bottom], [right, top]];
    }

    protected recalculateIndices(): void {
        let dim = this.boundDimensions == 'x' ? this.origin.getActualXAxis() : this.origin.getActualYAxis();

        let indices: number[] = [];
        for (let i = 0; i < dim.data.length; i++) {
            let d = dim.data[i].value;
            if (d == this.category) {
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
        return this._color;
    }

    public toJson(): any {
        let jFilter = super.toJson();
        jFilter.color = this.color;
        jFilter.category = this.category;

        return jFilter;
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        let filter = new CategoryFilter(jFilter.id);
        filter.applyJsonProperties(jFilter, origin);
        return filter;
    }

    protected applyJsonProperties(jFilter: any, origin: Graph): void {
        super.applyJsonProperties(jFilter, origin);
        this._color = jFilter.color;
        this._category = jFilter.category;
        this.recalculateIndices();
    }
}
