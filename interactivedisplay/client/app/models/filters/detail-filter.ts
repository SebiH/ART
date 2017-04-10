import { Filter } from './filter';
import { Graph } from '../graph';

export class DetailFilter extends Filter {

    public addPathPoint(p: [number, number]): void {
        this._path.push(p);
    }

    public clearPath(): void {
        this._path = [];
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
