import { Component, HostListener, OnInit } from '@angular/core';
import { SurfaceProvider, SocketIO, ServiceLoader } from '../../services/index';

@Component({
    selector: 'surface-container',
    template: `<graph-list *ngIf="isLoaded"></graph-list>
        <div ngIf="!isLoaded"> <h1> Initializing... </h1> </div>`,
    styles: [ 'div { height: 100%; width: 100%; display: flex; justify-content: center; align-items: center; }' ]
})

export class SurfaceComponent implements OnInit {
    private isLoaded: boolean = false;

    constructor (
        private surfaceProvider: SurfaceProvider,
        private socketio: SocketIO,
        private serviceLoader: ServiceLoader) {
        socketio.connect();
        this.disableBackNavigation();
    }

    ngOnInit() {
        this.serviceLoader.onLoaded()
            .first()
            .subscribe(() => this.isLoaded = true);
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
