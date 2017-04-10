import { Filter } from './filter';
import { Graph } from '../graph';
import { GradientStop } from '../chart-dimension';

export class MetricFilter extends Filter {

    public gradient: GradientStop[] = [];

    private _range: [number, number] = [0, 0];
    public get range(): [number, number] {
        return this._range;
    }
    public set range(v: [number, number]) {
        if (this._range !== v) {
            let sortedRange: [number, number] = [
                Math.min(this._range[0]),
                Math.max(this._range[1])
            ];

            this._range = sortedRange;
            this.generatePath();
        }
    }

    // todo
    private generatePath(): void {

    }


    public toJson(): any {
        let jFilter = super.toJson();
        jFilter.range = this.range;
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
        this._range = jFilter.range;
        this.gradient = jFilter.gradient;
    }
}
