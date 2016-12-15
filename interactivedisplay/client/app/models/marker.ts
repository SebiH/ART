import { Utils } from '../Utils';
import * as _ from 'lodash';

export class Marker {
    private _id: number;
    public get id(): number {
        return this._id;
    }
    public set id(id: number) {
        this._id = id;
        this.delayedRaisePropertyChangedEvent();
    }

    private _posX: number;
    public get posX(): number {
        return this._posX;
    }
    public set posX(posX: number) {
        this._posX = posX;
        this.delayedRaisePropertyChangedEvent();
    }

    private _posY: number;
    public get posY(): number {
        return this._posY;
    }
    public set posY(posY: number) {
        this._posY = posY;
        this.delayedRaisePropertyChangedEvent();
    }

    private _size: number;
    public get size(): number {
        return this._size;
    }
    public set size(size: number) {
        this._size = size;
        this.delayedRaisePropertyChangedEvent();
    }

    public get src(): string {
        return './markers/artoolkitplusbch_' + Utils.padLeft('' + this.id, 5) + '.png';
    }

    public constructor() {
        this.delayedRaisePropertyChangedEvent = _.debounce(this.raisePropertyChangedEvent);
    }

    private propertyChangedHandlers: Function[] = [];

    private raisePropertyChangedEvent(): void {
        for (let handler of this.propertyChangedHandlers) {
            handler(this);
        }
    }

    private delayedRaisePropertyChangedEvent: Function;

    public onPropertyChanged(f: Function): void {
        this.propertyChangedHandlers.push(f);
    }

    public offPropertyChanged(f: Function): void {
        _.pull(this.propertyChangedHandlers, f);
    }


    public toJson(): any {
        return {
            id: this._id,
            posX: this._posX,
            posY: this._posY,
            size: this._size
        };
    }

    public fromJson(json: any): void {
        this._id = json.id;
        this._posX = json.posX;
        this._posY = json.posY;
        this._size = json.size;
    }
}
