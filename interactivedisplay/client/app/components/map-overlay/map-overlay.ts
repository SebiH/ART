import { Component, OnInit } from '@angular/core';
import { SocketIO, MapProvider } from '../../services/index';
import { Map } from '../../models/index'

@Component({
    selector: 'map-overlay',
    templateUrl: './app/components/map-overlay/map-overlay.html',
    styleUrls: [ './app/components/map-overlay/map-overlay.css' ]
})
export class MapOverlayComponent implements OnInit  {

    maps: Map[];
    // white border around maps for better detection
    borderSize: number = 20;

    constructor(
        private socketio: SocketIO,
        private mapProvider: MapProvider) {}

    ngOnInit() {
        this.maps = this.mapProvider.getMaps();
    }

    getMapImageSource(map: Map): string {
        return '/markermaps/map_ar_bch_' + map.id  + '.png';
    }
}
