import { Filter } from './filter';
import { Graph } from '../graph';

export class DetailFilter extends Filter {

    public addPathPoint(p: [number, number]): void {
        this.path.push(p);
        this.propagateUpdates(['path']);
    }

    public clearPath(): void {
        this.path = [];
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
