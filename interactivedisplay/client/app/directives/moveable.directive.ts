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
        this.moveStart.emit();
    }

    private handleTouchMove(ev: InteractionEvent) {
        this.moveUpdate.emit({
            deltaX: ev.delta.x,
            deltaY: ev.delta.y,
        });
    }

    private handleTouchUp(ev: InteractionEvent) {
        this.moveEnd.emit();
    }
}
