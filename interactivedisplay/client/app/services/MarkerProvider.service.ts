import { Injectable, HostListener } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Marker } from '../models/index';

/**
 *    TODO: 
 *        - Send marker data to unity
 *        - Get marker arrangement from unity (low priority)
 */

@Injectable()
export class MarkerProvider {
    private markers: Marker[] = [];

    constructor(private socketio: SocketIO) {
        // socketio.on('marker', (markers) => { this.updateMarkers(markers) });
    }

    public initMarkers() {
        while (this.markers.length > 0) {
            this.markers.pop();
        }

        let markerSize = 100;
        let borderSize = 20 * 2;
        // topleft
        this.markers.push({
            id: 0,
            posX: 0,
            posY: 0,
            size: markerSize
        });
        // bottom left
        this.markers.push({
            id: 1,
            posX: 0,
            posY: window.innerHeight - markerSize - borderSize,
            size: markerSize
        });

        // bottom right
        this.markers.push({
            id: 2,
            posX: window.innerWidth - markerSize - borderSize,
            posY: window.innerHeight - markerSize - borderSize,
            size: markerSize
        });

        // top right
        this.markers.push({
            id: 3,
            posX: window.innerWidth - markerSize - borderSize,
            posY: 0,
            size: markerSize
        });
    }

    private updateMarkers(markers) {
        while (this.markers.length > 0) {
            this.markers.pop();
        }
    }

    public getMarkers(): Marker[] {
        return this.markers;
    }
}

