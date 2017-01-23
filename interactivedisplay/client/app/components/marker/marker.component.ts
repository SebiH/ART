import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { Marker, MARKER_SIZE_PX, Point } from '../../models/index';

@Component({
    selector: 'ar-marker',
    templateUrl: './app/components/marker/marker.html',
    styleUrls: [ './app/components/marker/marker.css' ]
})
export class MarkerComponent implements OnInit, OnDestroy
{
    @Input()
    private marker: Marker;

    @ViewChild('markerElement')
    private markerElement: ElementRef;

    private markerSize = MARKER_SIZE_PX;
    private timerSubscription: Subscription;

    constructor() { }

    ngOnInit() {
        let timer = Observable.timer(0, 100);
        this.timerSubscription = timer.subscribe(this.checkForChanges.bind(this));
    }

    ngOnDestroy() {
        this.timerSubscription.unsubscribe();
    }

    private checkForChanges() {
        let markerPosition = this.getMarkerPosition();
        this.marker.position = markerPosition;
    }

    private getMarkerPosition(): Point {
        let element = <HTMLElement>this.markerElement.nativeElement;
        let pos = element.getBoundingClientRect();

        return new Point(pos.left, pos.top);
    }
}
