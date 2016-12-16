import { Utils } from '../Utils';
import * as _ from 'lodash';

export class Marker {
    private _id: number;
    public get id(): number {
        return this._id;
    }

    public get src(): string {
        return './markers/artoolkitplusbch_' + Utils.padLeft('' + this.id, 5) + '.png';
    }

    public constructor(id: number) {
        this._id = id;
    }
}
