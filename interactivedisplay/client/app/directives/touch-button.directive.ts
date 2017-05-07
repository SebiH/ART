import { Directive, OnInit, OnDestroy, EventEmitter, Output, ElementRef, Input } from '@angular/core';
import { InteractionManager, InteractionEvent, InteractionListener, InteractionEventType } from '../services/index';
import { Point } from '../models/index';

@Directive({
    selector: '[touch-button]'
})
export class TouchButtonDirective implements OnInit, OnDestroy {

    @Output() touchclick = new EventEmitter();
    @Output() touchpress = new EventEmitter();
    @Input() usePress: boolean = false;

    private clickListener: InteractionListener;
    private pressDownListener: InteractionListener;
    private pressUpListener: InteractionListener;

    constructor(
        private interactions: InteractionManager,
        private elementRef: ElementRef) {}

    ngOnInit() {
        this.clickListener = {
            type: InteractionEventType.Click,
            element: this.elementRef.nativeElement,
            handler: ev => this.trigger(ev, this.touchclick)
        };

        this.pressDownListener = {
            type: InteractionEventType.PressDown,
            element: this.elementRef.nativeElement,
            handler: ev => this.trigger(ev, this.touchpress)
        };

        this.pressUpListener = {
            type: InteractionEventType.PressUp,
            element: this.elementRef.nativeElement,
            handler: ev => { /* do nothing */ }
        };

        this.interactions.on(this.clickListener);
        if (this.usePress) {
            this.interactions.on(this.pressDownListener);
            this.interactions.on(this.pressUpListener);
        }
    }

    private trigger(event: InteractionEvent, emitter: EventEmitter<{}>) {
        let elementOrigin = this.getElementPos();
        let relativePos = Point.sub(event.position, elementOrigin);

        emitter.emit({
            pos: event.position,
            relativePos: relativePos
        });
    }

    ngOnDestroy() {
        this.interactions.off(this.clickListener);
        if (this.usePress) {
            this.interactions.off(this.pressDownListener);
            this.interactions.off(this.pressUpListener);
        }
    }

    private getElementPos(): Point {
        let pos = this.elementRef.nativeElement.getBoundingClientRect();
        return new Point(pos.left, pos.top);
    }
}
