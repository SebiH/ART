import { Filter } from './filter';
import { Graph } from '../graph';
import { GradientStop } from '../chart-dimension';

interface Range {
    readonly min: number,
    readonly max: number
}

export class MetricFilter extends Filter {

    public gradient: GradientStop[] = [];

    private _range: Range = { min: 0, max: 0 };
    public get range(): Range {
        return this._range;
    }
    public set range(v: Range) {
        if (v.min != this._range.min || v.max != this._range.max) {
            this._range = v;
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

    // todo
    private generatePath(): void {

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
        this.setRange(jFilter.range);
        this.gradient = jFilter.gradient;
    }
}
