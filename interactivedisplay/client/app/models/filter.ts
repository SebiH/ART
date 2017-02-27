import { Graph } from './graph';

export type FilterPoint = [number, number];

export class Filter {
    public indices: number[];
    public path: FilterPoint[];
    public origin: Graph;
    public isColored: boolean;
    public color: string;

    public toJson(): any {
        return {
            indices: this.indices,
            path: this.path,
            origin: this.origin.id,
            isColored: this.isColored,
            color: this.color
        };
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        var filter = new Filter();
        filter.indices = jFilter.indices;
        filter.path = jFilter.indices;
        filter.isColored = jFilter.isColored;
        filter.color = jFilter.color;

        filter.origin = origin;

        return filter;
    }
}
