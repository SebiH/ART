import { Component, OnInit } from '@angular/core';
import { SocketIO, MarkerProvider } from '../../services/index';
import { Marker } from '../../models/index'

@Component({
    selector: 'marker-overlay',
    templateUrl: './app/components/marker-overlay/marker-overlay.html',
    styleUrls: [ './app/components/marker-overlay/marker-overlay.css' ]
})
export class MarkerOverlayComponent implements OnInit  {

    markers: Marker[];
    // white border around markers for better detection
    borderSize: number = 20;

    constructor(
        private socketio: SocketIO,
        private markerProvider: MarkerProvider) {}

    ngOnInit() {
        this.markers = this.markerProvider.getMarkers();
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
