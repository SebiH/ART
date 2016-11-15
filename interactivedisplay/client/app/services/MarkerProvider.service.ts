import { Injectable, HostListener } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Marker } from '../models/index';

/**
 *    TODO: 
 *        - Get marker arrangement from unity (low priority)
 */

@Injectable()
export class MarkerProvider {
    private markers: Marker[] = [];

    constructor(private socketio: SocketIO) { }

    public initMarkers() {
        while (this.markers.length > 0) {
            let marker = this.markers.pop();
            marker.offPropertyChanged((marker) => this.syncMarker(marker));
        }

        let markerSize = 200;
        let borderSize = 50;
        let currentId = 0;
        let markerCount = 5;

        let width = window.innerWidth - borderSize * 2;
        let height = window.innerHeight - borderSize * 2;

        // distribute markers on top
        for (let i = 0; i < markerCount; i++) {
            for (let j = 0; j < markerCount; j++) {
                let marker = new Marker();
                marker.id = currentId++;
                marker.posX = Math.max(i / (markerCount - 1) * width - markerSize, 0) + borderSize;
                marker.posY = Math.max(j / (markerCount - 1) * height - markerSize, 0) + borderSize;
                marker.size = markerSize;
                marker.onPropertyChanged((marker) => this.syncMarker(marker));
                this.markers.push(marker);
            }
        }
    }

    private syncMarker(marker: Marker) {
        this.socketio.sendMessage('marker', marker.toJson());
    }

    public getMarkers(): Marker[] {
        return this.markers;
    }
}

