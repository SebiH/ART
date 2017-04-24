import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Utils } from '../Utils';
import { Point } from './point';
import * as _ from 'lodash';

export const MARKER_SIZE_PX = 250;
export const MARKER_COUNT = 512;

export class Marker {
    private _id: number;
    public get id(): number {
        return this._id;
    }

    public get src(): string {
        return './markers/' + Utils.padLeft('' + this.id, 3) + '.png';
    }

    private _position: Point = new Point(0, 0);
    public get position(): Point {
        return this._position;
    }

    public set position(val: Point) {
        if (!val.equalTo(this._position)) {
            this._position = val;
            this.raiseChangeEvent();
        }
    }

    private changeSubscription: Subject<any> = new Subject<any>();
    public get onChange(): Observable<any> {
        return this.changeSubscription.asObservable();
    }

    public constructor(id: number) {
        this._id = id;
    }


    public toJson(): any {
        return {
            id: this._id,
            posX: this._position.x,
            posY: this._position.y,
            size: MARKER_SIZE_PX
        }
    }

    private raiseChangeEvent() {
        this.changeSubscription.next();
    }
}
