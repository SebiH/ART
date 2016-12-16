import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Marker } from '../../models/index';
import { SocketIO } from '../../services/index';

const MARKER_SIZE_PX = 300;

@Component({
    selector: 'ar-marker',
    templateUrl: './app/components/marker/marker.html',
    styleUrls: [ './app/components/marker/marker.css' ]
})
export class MarkerComponent implements OnInit, OnDestroy
{
    @Input() private marker: Marker;
    private markerSize = MARKER_SIZE_PX;

    constructor(private socketio: SocketIO) { }

    ngOnInit() {
    }

    ngOnDestroy() {
    }
}
