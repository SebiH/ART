import { Graph } from './graph';

export type FilterPoint = [number, number];

export class Filter {
    public readonly id: number;
    public indices: number[] = [];
    public path: FilterPoint[] = [];
    public origin: Graph = null;
    public isColored: boolean = false;
    public color: string = "#FFFFFF";

    constructor(id: number) {
        this.id = id;
    }

    public toJson(): any {
        return {
            id: this.id,
            indices: this.indices,
            path: this.path,
            origin: this.origin.id,
            isColored: this.isColored,
            color: this.color
        };
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        var filter = new Filter(jFilter.id);
        filter.indices = jFilter.indices;
        filter.path = jFilter.indices;
        filter.isColored = jFilter.isColored;
        filter.color = jFilter.color;

        filter.origin = origin;

        return filter;
    }
}
