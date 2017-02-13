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
    }

    @HostListener('window:resize', ['$event'])
    private onResize(event) {
        this.surfaceProvider.setSize(window.innerWidth, window.innerHeight);
    }
}
