import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Point } from './point';

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
    private _dimX : string = "";
    public get dimX() : string {
        return this._dimX;
    }
    public set dimX(v : string) {
        if (this._dimX != v) {
            this._dimX = v;
            this.propagateUpdates(['dimX']);
        }
    }

    /*
     *    dimY
     */
    private _dimY : string = "";
    public get dimY() : string {
        return this._dimY;
    }
    public set dimY(v : string) {
        if (this._dimY != v) {
            this._dimY = v;
            this.propagateUpdates(['dimY']);
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
     *    absolutePos
     */
    private _absolutePos : number = 0;
    public get absolutePos() : number {
        return this._absolutePos;
    }
    public set absolutePos(v : number) {
        if (this._absolutePos != v) {
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
    private _width : number;
    public get width() : number {
        return this.isSelected ? window.innerWidth * 0.9 : this._width;
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

    public constructor(id: number) {
        this._id = id;
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
            
            dimX: this.dimX,
            dimY: this.dimY,
            color: this.color,
            isSelected: this.isSelected,
            isNewlyCreated: this.isNewlyCreated,

            pos: this.absolutePos,
            width: this.width
        }
    }


    // inverse of .toJson()
    public static fromJson(jGraph: any): Graph {
        let graph = new Graph();
        graph._id = jGraph.id;

        graph._dimX = jGraph.dimX;
        graph._dimY = jGraph.dimY;
        graph._color = jGraph.color;
        graph._isSelected = jGraph.isSelected;

        graph._absolutePos = jGraph.pos;
        graph._width = jGraph.width;

        return graph;
    }
}
