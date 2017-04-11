import { Filter } from './filter';
import { Graph } from '../graph';

const DEFAULT_FILTER_COLOUR = "#03A9F4";

export class DetailFilter extends Filter {

    public addPathPoint(p: [number, number]): void {
        this.path.push(p);
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
