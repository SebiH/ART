import { Directive, OnInit, OnDestroy, EventEmitter, Output, ElementRef } from '@angular/core';
import { InteractionManager, InteractionEvent, InteractionListener, InteractionEventType } from '../services/index';
import { Point } from '../models/index';

@Directive({
    selector: '[touch-button]'
})
export class TouchButtonDirective implements OnInit, OnDestroy {

    @Output() touchclick = new EventEmitter();

    private clickListener: InteractionListener;

    constructor(
        private interactions: InteractionManager,
        private elementRef: ElementRef) {}

    ngOnInit() {
        this.clickListener = {
            type: InteractionEventType.Click,
            element: this.elementRef.nativeElement,
            handler: ev => {
                let elementOrigin = this.getElementPos();
                let relativePos = Point.sub(ev.position, elementOrigin);

                this.touchclick.emit({
                    pos: ev.position,
                    relativePos: relativePos
                });
            }
        };
        this.interactions.on(this.clickListener);
    }

    ngOnDestroy() {
        this.interactions.off(this.clickListener);
    }

    private getElementPos(): Point {
        let pos = this.elementRef.nativeElement.getBoundingClientRect();
        return new Point(pos.left, pos.top);
    }
}
