import { Graph } from './graph';

export type FilterPoint = [number, number];

export enum FilterType {
    Categorical = 0,
    Metric = 1,
    Detail = 2
}

export class Filter {
    public readonly id: number;
    public indices: number[] = [];
    public origin: Graph = null;
    public color: string = "#03A9F4";
    public isOverview: boolean = false;

    // type determines which of the following three optional properties is set
    public type: FilterType = FilterType.Detail;
    // for 2d detail filters
    public path: FilterPoint[] = [];
    // for 1d categorical filters
    public category?: number;
    // for 1d metric chart filters
    public range?: [number, number];

    constructor(id: number) {
        this.id = id;
    }

    public toJson(): any {
        let sortedRange: [number,number];
        if (this.type == FilterType.Metric) {
            sortedRange = [
                Math.min(this.range[0]),
                Math.max(this.range[1])
            ];
        }

        let unityPath: number[] = [];
        for (let p of this.path) {
            unityPath.push(p[0]);
            unityPath.push(p[1]);
        }

        return {
            id: this.id,
            origin: this.origin.id,
            color: this.color,
            isOverview: this.isOverview,

            type: this.type,
            path: unityPath,
            category: this.category,
            range: sortedRange
        };
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        var filter = new Filter(jFilter.id);
        filter.color = jFilter.color;
        filter.isOverview = jFilter.isOverview;

        filter.type = <FilterType>jFilter.type;
        switch (filter.type) {
            case FilterType.Categorical:
                filter.category = jFilter.category;
                break;
            case FilterType.Metric:
                filter.range = jFilter.range;
                break;
            case FilterType.Detail:
                break;
        }
        
        filter.path = [];
        for (let i = 0; i < jFilter.path.length; i += 2) {
            filter.path.push([jFilter.path[i], jFilter.path[i + 1]])
        }

        filter.origin = origin;

        return filter;
    }
}
