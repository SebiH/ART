import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

export class Surface {

    /*
     *    Name
     */
    private _name : string = "Surface";
    public get name() : string {
        return this._name;
    }
    public set name(v : string) {
        if (this._name != v) {
            this._name = v;
            this.updateSubscription.next();
        }
    }

    /*
     *    Width
     */
    private _width : number;
    public get width() : number {
        return this._width;
    }
    public set width(v : number) {
        if (this._width != v) {
            this._width = v;
            this.updateSubscription.next();
        }
    }

    /*
     *    Height
     */
    private _height : number;
    public get height() : number {
        return this._height;
    }
    public set height(v : number) {
        if (this._height != v) {
            this._height = v;
            this.updateSubscription.next();
        }
    }


    /*
     *    PixelToCmRatio
     */
    private _pixelToCmRatio : number = 0.0485;
    public get pixelToCmRatio() : number {
        return this._pixelToCmRatio;
    }
    public set pixelToCmRatio(v : number) {
        if (this._pixelToCmRatio != v) {
            this._pixelToCmRatio = v;
            this.updateSubscription.next();
        }
    }


    /*
     *    Offset
     */
    private _offset : number = 0.65;
    public get offset() : number {
        return this._offset;
    }
    public set offset(v : number) {
        if (this._offset != v) {
            this._offset = v;
            this.updateSubscription.next();
        }
    }


    public constructor() {
        this._width = window.innerWidth;
        this._height = window.innerHeight;
    }


    private updateSubscription: Subject<void> = new Subject<void>();
    public get onUpdate() : Observable<void> {
        return this.updateSubscription.asObservable();
    }


    public toJson(): any {
        return {
            name: this._name,
            pixelToCmRatio: this._pixelToCmRatio,
            width: this._width,
            height: this._height,
            offset: this._offset
        }
    }
}
