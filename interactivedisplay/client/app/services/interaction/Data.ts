import {Point} from '../../models/index';

import {InteractionType} from './Type';

export class InteractionData {
    public startPos: Point;
    public prevPos: Point;
    public currPos: Point;

    public totalDistance: Point = new Point(0, 0);

    public element: HTMLElement;

    public timeoutId: number = -1;

    public type: InteractionType = InteractionType.Undecided;
    // to avoid sending press events with press timer
    public isActive: boolean = true;

    public constructor(pos: Point, isActive: boolean = true) {
        this.startPos = pos;
        this.prevPos = pos;
        this.currPos = pos;
        this.isActive = isActive;
    }

    public addTotalDistance(delta: Point): void {
        this.totalDistance.x += Math.abs(delta.x);
        this.totalDistance.y += Math.abs(delta.y);
    }
}
