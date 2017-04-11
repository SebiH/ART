import { Filter } from './filter';
import { Graph } from '../graph';

const DEFAULT_FILTER_COLOUR = "#03A9F4";

export class DetailFilter extends Filter {

    public addPathPoint(p: [number, number]): void {
        let x = Math.max(this.origin.dimX.getMinValue(), Math.min(p[0], this.origin.dimX.getMaxValue()));
        let y = Math.max(this.origin.dimY.getMinValue(), Math.min(p[1], this.origin.dimY.getMaxValue()));

        this.path.push([x, y]);
        this.propagateUpdates(['path']);
    }

    public clearPath(): void {
        this.path = [];
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
    }
}
