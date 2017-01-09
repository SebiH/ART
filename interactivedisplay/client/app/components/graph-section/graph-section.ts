import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { Marker, Graph, Point } from '../../models/index';
import { MarkerProvider, SocketIO } from '../../services/index';

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
    private marker: Marker;
    private prevPosition: Point;
    private timerSubscription: Subscription;
    private socketioListener;

    @ViewChild('moveElement') private moveElement: ElementRef;

    private backgroundColor: string = "white";

    constructor (
        private markerProvider: MarkerProvider,
        private socketio: SocketIO,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        this.marker = this.markerProvider.createMarker();
        this.backgroundColor = COLOURS[this.graph.id % COLOURS.length];

        let timer = Observable.timer(0, 50);
        this.timerSubscription = timer.subscribe(this.checkForChanges.bind(this));

        // this.socketioListener = () => { this.sendPosition(this.getSectionPosition()); };
        // this.socketio.on('get-plane', this.socketioListener);
    }

    ngOnDestroy() {
        this.markerProvider.destroyMarker(this.marker);
        this.timerSubscription.unsubscribe();
        // this.socketio.off('get-plane', this.socketioListener);
    }

    private checkForChanges() {
        let currentPosition = this.getSectionPosition();

        if (this.prevPosition == undefined || !this.prevPosition.equalTo(currentPosition)) {
            this.sendPosition(currentPosition);
            this.prevPosition = currentPosition;
        }
    }

    private getSectionPosition(): Point {
        let element = <HTMLElement>this.elementRef.nativeElement;
        let pos = element.getBoundingClientRect();

        return new Point(pos.left, pos.top);
    }

    private sendPosition(pos: Point) {
        // this.socketio.sendMessage('plane-position', {
            //   id: this.graph.id,
            //   pos: pos.x
            // });
        }
    }
