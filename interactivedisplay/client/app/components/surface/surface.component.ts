import { Component, HostListener } from '@angular/core';
import { SurfaceProvider, SocketIO } from '../../services/index';

@Component({
    selector: 'surface-container',
    templateUrl: './app/components/surface/surface.html',
    styleUrls: ['./app/components/surface/surface.css']
})

export class SurfaceComponent {
    constructor (private surfaceProvider: SurfaceProvider, private socketio: SocketIO) {
        socketio.connect();
        this.disableBackNavigation();
    }

    @HostListener('window:resize', ['$event'])
    private onResize(event) {
        this.surfaceProvider.setSize(window.innerWidth, window.innerHeight);
    }


    /*
     * Adapted from http://stackoverflow.com/a/12381873/4090817
     */
    private disableBackNavigation() {
        window.location.href += "#";

        setTimeout(function () {
            window.location.href += "!";
        }, 50); 
    }

    @HostListener('window:hashchange', ['$event'])
    private onHashChange(event) {
        if (window.location.hash !== "!") {
            window.location.hash = "!";
        }
    }
}
