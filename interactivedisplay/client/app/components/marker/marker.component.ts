import { Component, Input, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { Marker, Point } from '../../models/index';
import { SocketIO } from '../../services/index';

const MARKER_SIZE_PX = 250;

@Component({
    selector: 'ar-marker',
    templateUrl: './app/components/marker/marker.html',
    styleUrls: [ './app/components/marker/marker.css' ]
})
export class MarkerComponent implements OnInit, OnDestroy
{
    @Input() private marker: Marker;
    private markerSize = MARKER_SIZE_PX;

    private socketioListener;
    private timerSubscription: Subscription;
    private prevMarkerPos: Point;

    constructor(private socketio: SocketIO, private elementRef: ElementRef) { }

    ngOnInit() {
        let timer = Observable.timer(0, 100);
        this.timerSubscription = timer.subscribe(this.checkForChanges.bind(this));
    }

    ngOnDestroy() {
        this.timerSubscription.unsubscribe();
    }

    private checkForChanges() {
        let markerPosition = this.getMarkerPosition();

        if (this.prevMarkerPos == undefined || !this.prevMarkerPos.equalTo(markerPosition)) {
            this.sendMarkerPosition(markerPosition);
            this.prevMarkerPos = markerPosition;
        }
    }

    private getMarkerPosition(): Point {
        let element = <HTMLElement>this.elementRef.nativeElement;
        let pos = element.getBoundingClientRect();

        return new Point(pos.left, pos.top);
    }

    private sendMarkerPosition(pos: Point) {
        this.socketio.sendMessage('marker', {
            id: this.marker.id,
            posX: pos.x,
            posY: pos.y,
            size: this.markerSize
        });
    }
}
