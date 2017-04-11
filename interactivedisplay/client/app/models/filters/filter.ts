import { Graph } from '../graph';

export abstract class Filter {
    public readonly id: number;

    public origin: Graph = null;
    public boundDimensions: 'x' | 'y' | 'xy';
    public selectedDataIndices: number[] = [];
    public isUserGenerated: boolean = false;

    protected _path : [number, number][] = [];
    public get path() : [number, number][] {
        return this._path;
    }



    constructor(id: number) {
        this.id = id;
    }

    public toJson(): any {
        let unityPath: number[] = [];
        for (let p of this.path) {
            unityPath.push(p[0]);
            unityPath.push(p[1]);
        }

        return {
            id: this.id,
            origin: this.origin.id,
            boundDimensions: this.boundDimensions,
            isUserGenerated: this.isUserGenerated,
            path: unityPath
        };
    }

    protected applyJsonProperties(jFilter: any, origin: Graph): void {
        this.origin = origin;
        this.boundDimensions = jFilter.boundDimensions;
        this._path = jFilter.path;
        this.isUserGenerated = jFilter.isUserGenerated;
    }
}
