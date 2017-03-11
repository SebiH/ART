import { Graph } from './graph';

export type FilterPoint = [number, number];

export enum FilterType {
    Categorical = 0,
    Line = 1,
    Detail = 2
}

export class Filter {
    public readonly id: number;
    public indices: number[] = [];
    public origin: Graph = null;
    public color: string = "#FFFFFF";
    public isOverview: boolean = false;


    // type determines which of the following three optional properties is set
    public type: FilterType = FilterType.Detail;
    // for 2d detail filters
    public path?: FilterPoint[];
    // for 1d categorical filters
    public category?: number;
    // for 1d line chart filters
    public range?: [number, number];

    constructor(id: number) {
        this.id = id;
    }

    public toJson(): any {
        return {
            id: this.id,
            indices: this.indices,
            origin: this.origin.id,
            color: this.color,
            isOverview: this.isOverview,

            type: this.type,
            path: this.path,
            category: this.category,
            range: this.range
        };
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        var filter = new Filter(jFilter.id);
        filter.indices = jFilter.indices;
        filter.color = jFilter.color;
        filter.isOverview = jFilter.isOverview;

        filter.type = <FilterType>jFilter.type;
        switch (filter.type) {
            case FilterType.Categorical:
                filter.category = jFilter.category;
                break;
            case FilterType.Line:
                filter.range = jFilter.range;
                break;
            case FilterType.Detail:
                filter.path = jFilter.path;
                break;
        }

        filter.origin = origin;

        return filter;
    }
}
