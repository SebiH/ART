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
        }
    }


    private generatePath(): void {
        let left, right, top, bottom;

        if (this.boundDimensions == 'x') {
            left = this.category - 0.5;
            right = this.category + 0.5;
            top = this.origin.dimY.getMaxValue();
            bottom = this.origin.dimY.getMinValue();
        } else {
            left = this.origin.dimX.getMinValue();
            right = this.origin.dimX.getMaxValue();
            top = this.category - 0.5;
            bottom = this.category + 0.5;
        }

        this.path = [[left, top], [left, bottom], [right, bottom], [right, top]];
    }



    public onDimensionChanged(prevDimX: ChartDimension, prevDimY: ChartDimension): void {
        this.generatePath();
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
    }
}
