import { Injectable, HostListener, OnInit } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Marker } from '../models/index';

import * as _ from 'lodash';

@Injectable()
export class MarkerProvider implements OnInit {
    private markers: Marker[] = [];
    private idCounter: number = 0;

    constructor(private socketio: SocketIO) { }

    ngOnInit() {
    }

    private addMarkerUnity(marker: Marker) {
        this.socketio.sendMessage('add-marker', marker.id);
    }

    private removeMarkerUnity(marker: Marker) {
        this.socketio.sendMessage('remove-marker', marker.id);
    }

    public getMarkers(): Marker[] {
        return this.markers;
    }

    public createMarker(): Marker {
        let marker: Marker = new Marker(this.idCounter++);
        this.addMarkerUnity(marker);
        return marker;
    }

    public destroyMarker(marker: Marker) {
        _.pull(this.markers, marker);
        this.removeMarkerUnity(marker);
    }
}

