import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { ChartDimension } from './chart-dimension';
import { DataProvider } from '../services/index';

import * as _ from 'lodash';

const DEFAULT_GRAPH_WIDTH = 1250;

export class Graph {
    /*
     *    Id
     */
    private _id: number = -1;
    public get id(): number {
        return this._id;
    }

    /*
     *    dimX
     */
    private _dimX: ChartDimension = null;
    private get dimX() : ChartDimension {
        return this._dimX;
    }
    private set dimX(v : ChartDimension) {
        if (this._dimX != v) {
            this._dimX = v;
            this.sortedAxisX = null;
            this.propagateUpdates(['dimX']);
        }
    }
    // because getter/setter cannot have different accessibility levels
    public setDimX(v: ChartDimension): void {
        this.dimX = v;
    }

    /*
     *    dimY
     */
    private _dimY: ChartDimension = null;
    private get dimY() : ChartDimension {
        return this._dimY;
    }
    private set dimY(v : ChartDimension) {
        if (this._dimY != v) {
            this._dimY = v;
            this.sortedAxisX = null;
            this.propagateUpdates(['dimY']);
        }
    }
    // because getter/setter cannot have different accessibility levels
    public setDimY(v: ChartDimension): void {
        this.dimY = v;
    }

    /*
     *    phase
     */
    private _phase: string = null;
    public get phase() : string {
        return this._phase;
    }
    public set phase(v : string) {
        if (this._phase != v) {
            this.dimY = null;
            this.dimX = null;
            this._phase = v;
            this.propagateUpdates(['phase']);
        }
    }

    /*
     *    color
     */
    private _color : string = "#FFFFFF";
    public get color() : string {
        return this._color;
    }
    public set color(v : string) {
        if (this._color != v) {
            this._color = v;
            this.propagateUpdates(['color']);
        }
    }

    /*
     *    isColored
     */
    private _isColored : boolean = false;
    public get isColored() : boolean {
        return this._isColored;
    }
    public set isColored(v : boolean) {
        if (this._isColored != v) {
            this._isColored = v;
            this.propagateUpdates(['isColored']);
        }
    }

    /*
     *    isSelected
     */
    private _isSelected : boolean = false;
    public get isSelected() : boolean {
        return this._isSelected;
    }
    public set isSelected(v : boolean) {
        if (this._isSelected != v) {
            this._isSelected = v;
            this.propagateUpdates(['isSelected']);
        }
    }

    /*
     *     isFlipped
     */
    private _isFlipped : boolean = false;
    public get isFlipped() : boolean {
        return this._isFlipped;
    }
    public set isFlipped(v : boolean) {
        if (this._isFlipped != v) {
            this._isFlipped = v;
            this.propagateUpdates(['isFlipped']);
        }
    }

    /*
     *    absolutePos
     */
    private _absolutePos : number = 0;
    public get absolutePos() : number {
        return this._absolutePos;
    }
    public set absolutePos(v : number) {
        if (this._absolutePos != v && !isNaN(v)) {
            this._absolutePos = v;
            this.propagateUpdates(['absolutePos']);
        }
    }

    /*
     *    listIndex
     */
    private _listIndex : number = -1;
    public get listIndex() : number {
        return this._listIndex;
    }
    public set listIndex(v : number) {
        if (this._listIndex != v) {
            this._listIndex = v;
            this.propagateUpdates(['listIndex']);
        }
    }

    /*
     *    width
     */
    private _width : number = DEFAULT_GRAPH_WIDTH;
    public get width() : number {
        return this.isSelected ? window.innerWidth * 0.7 : this._width;
    }
    public set width(v : number) {
        if (this._width != v) {
            this._width = v;
            this.propagateUpdates(['width']);
        }
    }

    /*
     *    posOffset
     */
    private _posOffset : number = 0;
    public get posOffset() : number {
        return this._posOffset;
    }
    public set posOffset(v : number) {
        if (this._posOffset != v) {
            this._posOffset = v;
            this.propagateUpdates(['posOffset']);
        }
    }

    /*
     *    isPickedUp
     */
    private _isPickedUp : boolean = false;
    public get isPickedUp() : boolean {
        return this._isPickedUp;
    }
    public set isPickedUp(v : boolean) {
        if (this._isPickedUp != v) {
            this._isPickedUp = v;
            this.propagateUpdates(['isPickedUp', 'width']);
        }
    }

    /*
     *    isNewlyCreated
     */
    private _isNewlyCreated : boolean = false;
    public get isNewlyCreated() : boolean{
        return this._isNewlyCreated;
    }
    public set isNewlyCreated(v : boolean) {
        if (this._isNewlyCreated != v) {
            this._isNewlyCreated = v;
            this.propagateUpdates(['isNewlyCreated'])
        }
    }

    /*
     *    sortAxis
     */
    private _sortAxis : boolean;
    public get sortAxis() : boolean {
        return this._sortAxis;
    }
    public set sortAxis(v : boolean) {
        if (this._sortAxis != v) {
            this._sortAxis = v;
            this.propagateUpdates(['sortAxis', 'dimX']);
        }
    }

    public constructor(id: number) {
        this._id = id;
    }


    private sortedAxisX: ChartDimension = null;

    public getCurrentXAxis(): ChartDimension {
        if (this.isFlipped) {
            return this.getActualYAxis();
        } else {
            return this.getActualXAxis();
        }
    }

    public getCurrentYAxis(): ChartDimension {
        if (this.isFlipped) {
            return this.getActualXAxis();
        } else {
            return this.getActualYAxis();
        }
    }

    public getActualXAxis(): ChartDimension {
        if (this.sortAxis && this.dimX) {
            if (!this.sortedAxisX) {
                this.sortedAxisX = this.dimX.clone();
                this.sortedAxisX.sortBy(this.dimY);
            }

            return this.sortedAxisX;
        } else {
            return this.dimX;
        }
    }

    public getActualYAxis(): ChartDimension {
        return this.dimY;
    }


    private updateSubscription: Subject<string[]> = new Subject<string[]>();
    public get onUpdate() : Observable<string[]> {
        return this.updateSubscription.asObservable();
    }

    private propagateUpdates(changes: string[]) {
        this.updateSubscription.next(changes);
    }

    public destroy(): void {
        this.updateSubscription.complete();
    }

    public toJson(): any {
        return {
            id: this.id,

            dimX: (this.dimX ? this.dimX.name : ''),
            dimY: (this.dimY ? this.dimY.name : ''),
            phase: this.phase,
            color: this.color,
            isColored: this.isColored,
            isFlipped: this.isFlipped,
            isNewlyCreated: this.isNewlyCreated,
            isPickedUp: this.isPickedUp,
            isSelected: this.isSelected,
            sortAxis: this.sortAxis,

            pos: this.absolutePos,
            width: this.width
        }
    }


    // inverse of .toJson()
    public static fromJson(jGraph: any, provider: DataProvider): Graph {
        let graph = new Graph(jGraph.id);

        if (jGraph.dimX) {
            provider.getData(jGraph.dimX)
                .first()
                .subscribe(data => graph.dimX = data);
        }

        if (jGraph.dimY) {
            provider.getData(jGraph.dimY)
                .first()
                .subscribe(data => graph.dimY = data);
        }

        graph._phase = jGraph.phase;
        graph._color = jGraph.color;
        graph._isColored = jGraph.isColored;
        graph._isSelected = jGraph.isSelected;
        graph._isFlipped = jGraph.isFlipped;
        graph._sortAxis = jGraph.sortAxis;

        // force newly created to false because touch events won't persist
        graph._isNewlyCreated = false;

        graph._absolutePos = jGraph.pos;
      // graph._width = jGraph.width;
        // overwrite width since scaling isn't implemented
        graph._width = DEFAULT_GRAPH_WIDTH;

        return graph;
    }
}
