import * as _ from 'lodash';

export class Map {
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

    private _sizeX: number;
    public get sizeX(): number {
        return this._sizeX;
    }
    public set sizeX(sizeX: number) {
        this._sizeX = sizeX;
        // TODO: remove hardcoded values?
        this._sizeY = sizeX * 0.73913043478;
        this.delayedRaisePropertyChangedEvent();
    }

    private _sizeY: number;
    public get sizeY(): number {
        return this._sizeY;
    }
    public set sizeY(sizeY: number) {
        this._sizeY = sizeY;
        // TODO: remove hardcoded values?
        this._sizeX = sizeY * 1.35294117647
        this.delayedRaisePropertyChangedEvent();
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
            sizeX: this._sizeX,
            sizeY: this._sizeY
        };
    }

    public fromJson(json: any): void {
        this._id = json.id;
        this._posX = json.posX;
        this._posY = json.posY;
        this._sizeX = json.sizeX;
    }
}
