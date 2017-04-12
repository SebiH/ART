import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { Graph } from '../graph';
import { ChartDimension } from '../chart-dimension';

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
            this.attachListener(v);
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
     *    isInvalid
     */
    private _isInvalid : boolean = false;
    public get isInvalid() : boolean {
        return this._isInvalid;
    }
    public set isInvalid(v : boolean) {
        if (this._isInvalid != v) {
            this._isInvalid = v;
            this.propagateUpdates(['isInvalid']);
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


    // for pseudo-deconstructor
    protected isActive: boolean = true;


    constructor(id: number) {
        this.id = id;
    }

    private attachListener(graph: Graph): void {
        let prevDimX = graph.dimX;
        let prevDimY = graph.dimY;
        graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('dimX') >= 0 || changes.indexOf('dimY') >= 0)
            .subscribe((changes) => {
                this.onDimensionChanged(prevDimX, prevDimY);
                prevDimX = graph.dimX;
                prevDimY = graph.dimY;
            });
    }


    private readonly updateSubscription: Subject<string[]> = new Subject<string[]>();
    public get onUpdate(): Observable<string[]> {
        return this.updateSubscription.asObservable();
    }

    protected propagateUpdates(changes: string[]) {
        if (this.isActive) {
            this.updateSubscription.next(changes);
        }
    }


    public abstract getColor(): string;
    protected abstract recalculateIndices(): void;
    protected abstract onDimensionChanged(prevDimX: ChartDimension, prevDimY: ChartDimension): void;

    public destroy(): void {
        this.isActive = false;
        this.updateSubscription.complete();
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
        this.attachListener(origin);
        this._boundDimensions = jFilter.boundDimensions;
        this._isUserGenerated = jFilter.isUserGenerated;

        this._path = [];
        for (var i = 0; i < jFilter.path.length; i += 2) {
            this._path.push([jFilter.path[i], jFilter.path[i + 1]]);
        }
    }
}
