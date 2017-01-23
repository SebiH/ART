import { Injectable, HostListener, OnInit } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Marker } from '../models/index';

import * as _ from 'lodash';

@Injectable()
export class MarkerProvider {
    private markers: Marker[] = [];
    private idCounter: number = 0;

    private delayedMarkerUpdate: Function;
    private markerUpdateQueue: {[id: number]: any} = {};

    constructor(private socketio: SocketIO) {
        // for debugging
        window['markers'] = this.markers;
        this.delayedMarkerUpdate = _.debounce(this.updateMarkers, 0);
        this.socketio.sendMessage('marker-clear', null);
    }

    public getMarkers(): Marker[] {
        return this.markers;
    }

    public createMarker(): Marker {
        let marker: Marker = new Marker(this.idCounter++);
        this.markers.push(marker);
        this.socketio.sendMessage('+marker', marker.toJson());
        marker.onChange
            .takeWhile(() => this.markers.indexOf(marker) > -1)
            .subscribe(() => this.queueMarkerUpdate(marker));
        return marker;
    }

    public destroyMarker(marker: Marker) {
        _.pull(this.markers, marker);
        delete this.markerUpdateQueue[marker.id];
        this.socketio.sendMessage('-marker', marker.id);
    }

    private queueMarkerUpdate(marker: Marker) {
        this.markerUpdateQueue[marker.id] = marker.toJson();
        this.delayedMarkerUpdate();
    }

    private updateMarkers(): void {
        let markers = _.values(this.markerUpdateQueue);

        if (markers.length > 0) {
            this.socketio.sendMessage('marker', {
                markers: markers
            });
            this.markerUpdateQueue = {};
        }
    }
}

