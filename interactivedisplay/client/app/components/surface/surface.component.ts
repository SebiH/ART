import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { SurfaceProvider, SocketIO, ServiceLoader, Settings, SettingsProvider } from '../../services/index';

@Component({
    selector: 'surface-container',
    template: `<graph-list *ngIf="isLoaded"></graph-list>
        <marker-overlay *ngIf="settings?.showMarkerOverlay"></marker-overlay>
        <div *ngIf="!isLoaded"> <h1> Initializing... </h1> </div>`,
    styles: [
        'div { height: 100%; width: 100%; display: flex; justify-content: center; align-items: center; }',
        'marker-overlay { position: absolute; left: 0; top: 0; width: 100%; height: 100%; z-index: 999; }'
    ]
})

export class SurfaceComponent implements OnInit, OnDestroy {
    private isLoaded: boolean = false;
    private isActive: boolean = true;
    private settings: Settings = null;

    constructor (
        private surfaceProvider: SurfaceProvider,
        private socketio: SocketIO,
        private settingsProvider: SettingsProvider,
        private serviceLoader: ServiceLoader) {
        socketio.connect();
    }

    ngOnInit() {
        this.settingsProvider.getCurrent()
            .takeWhile(() => this.isActive)
            .subscribe((settings) => this.settings = settings);

        this.serviceLoader.onLoaded()
            .first()
            .subscribe(() => {
                this.isLoaded = true;
                this.disableBackNavigation();
            });
    }

    ngOnDestroy() {
        this.isActive = false;
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
