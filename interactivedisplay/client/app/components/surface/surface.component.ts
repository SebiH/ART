import { Component, HostListener } from '@angular/core';
import { SurfaceProvider } from '../../services/index';

@Component({
    selector: 'surface-container',
    templateUrl: './app/components/surface/surface.html',
    styleUrls: ['./app/components/surface/surface.css']
})

export class SurfaceComponent {
    constructor (private surfaceProvider: SurfaceProvider) { }

    @HostListener('window:resize', ['$event'])
    private onResize(event) {
        this.surfaceProvider.setSize(window.innerWidth, window.innerHeight);
    }
}
