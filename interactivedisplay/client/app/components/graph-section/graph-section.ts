import { Component, Input, OnInit, OnDestroy, ElementRef, Output, EventEmitter } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { Marker, Graph, Point } from '../../models/index';
import { MarkerProvider } from '../../services/index';

const COLOURS = [
    // material colour palette, see https://material.io/guidelines/style/color.html
    "#F44336", // red
    "#9C27B0", // purple
    "#3F51B5", // indigo
    "#2196F3", // blue
    "#4CAF50", // green
    "#FFEB3B", // yellow
    "#FF9800", // orange
    "#9E9E9E", // grey
];

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

    private marker: Marker;
    private timerSubscription: Subscription;

    private backgroundColor: string = "white";

    constructor (
        private markerProvider: MarkerProvider,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        this.marker = this.markerProvider.createMarker();
        this.backgroundColor = COLOURS[this.graph.id % COLOURS.length];

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
}
