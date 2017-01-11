import { Component, Input, OnInit, OnDestroy, ElementRef, Output, EventEmitter } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { Marker, Graph, Point } from '../../models/index';
import { MarkerProvider } from '../../services/index';

@Component({
    selector: 'graph-section',
    templateUrl: './app/components/graph-section/graph-section.html',
    styleUrls: ['./app/components/graph-section/graph-section.css'],
})
export class GraphSectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;

    @Output() onDelete = new EventEmitter(); 
    @Output() onSelect = new EventEmitter(); 
    @Output() onMove = new EventEmitter(); 

    @Output() onMoveDown = new EventEmitter(); 
    @Output() onMoveUp = new EventEmitter(); 

    private marker: Marker;
    private timerSubscription: Subscription;

    private backgroundColor: string = "white";

    constructor (
        private markerProvider: MarkerProvider,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        this.marker = this.markerProvider.createMarker();

        let timer = Observable.timer(0, 50);
        this.timerSubscription = timer.subscribe(this.checkForChanges.bind(this));
    }

    ngOnDestroy() {
        this.markerProvider.destroyMarker(this.marker);
        this.timerSubscription.unsubscribe();
    }

    private checkForChanges(): void {
        let currentPosition = this.getSectionPosition().x;

        if (this.graph.absolutePos == undefined || this.graph.absolutePos !== currentPosition) {
            this.graph.absolutePos = currentPosition;
            this.graph.updatePosition();
        }
    }

    private getSectionPosition(): Point {
        let element = <HTMLElement>this.elementRef.nativeElement;
        let pos = element.getBoundingClientRect();

        return new Point(pos.left, pos.top);
    }


    private handleDelete(): void {
        this.onDelete.emit();
    }


    private handleSelect(): void {
        this.onSelect.emit();
    }

    private handleMoveStart(graph: Graph, event: any) {
        this.onMove.emit({
            start: true,
        });
    }

    private handleMoveUpdate(graph: Graph, event: any) {
        this.onMove.emit({
            delta: event.deltaX
        });
    }

    private handleMoveEnd(graph: Graph, event: any) {
        this.onMove.emit({
            end: true,
        });
    }

    private moveDown(): void {
        this.onMoveDown.emit();
    }

    private moveUp(): void {
        this.onMoveUp.emit();
    }
}
