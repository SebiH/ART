import {
    Directive,
    ElementRef,
    EventEmitter,
    Output,
    OnInit,
    OnDestroy
} from '@angular/core';
import {
    InteractionManager,
    InteractionEvent,
    InteractionListener,
    InteractionEventType
} from '../services/index';
import { Point } from '../models/index';

@Directive({
    selector: '[moveable]'
})
export class MoveableDirective implements OnInit, OnDestroy {

    @Output() moveStart = new EventEmitter(); 
    @Output() moveUpdate = new EventEmitter(); 
    @Output() moveEnd = new EventEmitter(); 

    private touchDownListener: InteractionListener;
    private touchMoveListener: InteractionListener;
    private touchUpListener: InteractionListener;

    constructor(
        private interactions: InteractionManager,
        private elementRef: ElementRef
        ) { }

    ngOnInit() {
        this.touchDownListener = {
            type: InteractionEventType.TouchDown,
            element: this.elementRef.nativeElement,
            handler: (ev) => { this.handleTouchDown(ev); }
        };
        this.interactions.on(this.touchDownListener);

        this.touchMoveListener = {
            type: InteractionEventType.TouchMove,
            element: this.elementRef.nativeElement,
            handler: (ev) => { this.handleTouchMove(ev); }
        };
        this.interactions.on(this.touchMoveListener);

        this.touchUpListener = {
            type: InteractionEventType.TouchUp,
            element: this.elementRef.nativeElement,
            handler: (ev) => { this.handleTouchUp(ev); }
        };
        this.interactions.on(this.touchUpListener);
    }

    ngOnDestroy() {
        this.interactions.off(this.touchDownListener);
        this.interactions.off(this.touchMoveListener);
        this.interactions.off(this.touchUpListener);
    }

    private handleTouchDown(ev: InteractionEvent) {
        let elementOrigin = this.getElementPos();
        let relativePos = Point.sub(ev.position, elementOrigin);
        this.moveStart.emit({
            pos: ev.position,
            relativePos: relativePos
        });
    }

    private handleTouchMove(ev: InteractionEvent) {
        let elementOrigin = this.getElementPos();
        let relativePos = Point.sub(ev.position, elementOrigin);

        this.moveUpdate.emit({
            pos: ev.position,
            relativePos: relativePos,
            deltaX: ev.delta.x,
            deltaY: ev.delta.y,
        });
    }

    private handleTouchUp(ev: InteractionEvent) {
        let elementOrigin = this.getElementPos();
        let relativePos = Point.sub(ev.position, elementOrigin);

        this.moveEnd.emit({
            pos: ev.position,
            relativePos: relativePos
        });
    }


    private getElementPos(): Point {
        let pos = this.elementRef.nativeElement.getBoundingClientRect();
        return new Point(pos.left, pos.top);
    }
}
