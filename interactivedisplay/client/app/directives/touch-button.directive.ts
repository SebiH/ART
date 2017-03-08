import { Directive, OnInit, OnDestroy, EventEmitter, Output, ElementRef } from '@angular/core';
import { InteractionManager, InteractionEvent, InteractionListener, InteractionEventType } from '../services/index';

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
            handler: ev => this.touchclick.emit()
        };
        this.interactions.on(this.clickListener);
    }

    ngOnDestroy() {
        this.interactions.off(this.clickListener);
    }
}
