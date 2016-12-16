import {Component, ViewEncapsulation, OnInit, HostListener} from '@angular/core';
import {Router} from '@angular/router';

import {MenuComponent} from '../menu/menu';
import {MarkerOverlayComponent} from '../marker-overlay/marker-overlay';
import {SocketIO, MarkerProvider, MapProvider} from '../../services/index';

@Component({
    selector: 'main-app',
    templateUrl: './app/components/app/app.html',
    styleUrls: ['./app/components/app/app.css'],
    encapsulation: ViewEncapsulation.None
})

export class AppComponent implements OnInit
{
    useMaps: boolean = false;

    constructor (private socketio: SocketIO, private markerProvider: MarkerProvider, private mapProvider: MapProvider) { }

    ngOnInit() {
        this.sendWindowSize();
    }

    @HostListener('window:resize', ['$event'])
    private onResize(event) {
        this.sendWindowSize();
    }

    private sendWindowSize() {
        if (this.useMaps)
            this.mapProvider.initMaps(); // TODO: not here.
        
        this.socketio.sendMessage('window-size', {
            width: window.innerWidth,
            height: window.innerHeight
        });
    }
}
