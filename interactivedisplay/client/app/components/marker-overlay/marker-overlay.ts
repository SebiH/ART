import { Component, OnInit } from '@angular/core';
import { SocketIO } from '../../services/index';
import { Marker } from '../../models/index'

@Component({
    selector: 'marker-overlay',
    templateUrl: './app/components/marker-overlay/marker-overlay.html',
    styleUrls: [ './app/components/marker-overlay/marker-overlay.css' ]
})
export class MarkerOverlayComponent implements OnInit  {

    markers: Marker[] = [];
    // white border around markers for better detection
    borderSize: number = 20;

    constructor(private socketio: SocketIO) {}

    ngOnInit() {
        let markerSize = 100;
        this.markers.push({
            id: 1,
            posX: 0,
            posY: 0,
            size: markerSize
        });

        this.markers.push({
            id: 2,
            posX: 500,
            posY: 500,
            size: markerSize
        });

        this.markers.push({
            id: 3,
            posX: 500,
            posY: 0,
            size: markerSize
        });

        this.markers.push({
            id: 4,
            posX: 0,
            posY: 500,
            size: markerSize
        });
    }

    getMarkerImageSource(marker: Marker): string {
        return '/markers/artoolkitplusbch_' + this.padLeft('' + marker.id, 5)  + '.png';
    }

    private padLeft(str: string, size: number) {
        while (str.length < size) {
            str = '0' + str;
        }
        return str;
    }
}
