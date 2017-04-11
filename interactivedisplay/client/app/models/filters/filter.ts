import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { Graph } from '../graph';

type BD = 'x' | 'y' | 'xy';

export abstract class Filter {
    public readonly id: number;

    /*
     *    origin
     */
    private _origin : Graph = null;
    public get origin() : Graph {
        return this._origin;
    }
    public set origin(v : Graph) {
        if (this._origin != v) {
            this._origin = v;
            this.propagateUpdates(['origin']);
        }
    }

    /*
     *    boundDimensions
     */
    private _boundDimensions : BD;
    public get boundDimensions() : BD {
        return this._boundDimensions;
    }
    public set boundDimensions(v : BD) {
        if (this._boundDimensions != v) {
            this._boundDimensions = v;
            this.propagateUpdates(['boundDimensions']);
        }
    }


    /*
     *    selectedDataIndices
     */
    private _selectedDataIndices : number[] = [];
    public get selectedDataIndices() : number[] {
        return this._selectedDataIndices;
    }
    public set selectedDataIndices(v : number[]) {
        if (this._selectedDataIndices != v) {
            this._selectedDataIndices = v;
            this.propagateUpdates(['selectedDataIndices']);
        }
    }

    /*
     *    isUserGenerated
     */
    private _isUserGenerated : boolean = true;
    public get isUserGenerated() : boolean {
        return this._isUserGenerated;
    }
    public set isUserGenerated(v : boolean) {
        if (this._isUserGenerated != v) {
            this._isUserGenerated = v;
            this.propagateUpdates(['isUserGenerated']);
        }
    }

    /*
     *    path
     */
    private _path : [number, number][] = [];
    public get path() : [number, number][] {
        return this._path;
    }
    public set path(v: [number, number][]) {
        if (this._path != v) {
            this._path = v;
            this.propagateUpdates(['path']);
        }
    }


    constructor(id: number) {
        this.id = id;
    }


    private readonly updateSubscription: Subject<string[]> = new Subject<string[]>();
    public get onUpdate(): Observable<string[]> {
        return this.updateSubscription.asObservable();
    }

    protected propagateUpdates(changes: string[]) {
        this.updateSubscription.next(changes);
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
        this._origin = origin;
        this._boundDimensions = jFilter.boundDimensions;
        this._path = jFilter.path;
        this._isUserGenerated = jFilter.isUserGenerated;
    }
}
