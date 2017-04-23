import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Marker, MARKER_SIZE_PX, Point } from '../../models/index';
import { MarkerProvider } from '../../services/index';

@Component({
    selector: 'ar-marker-test',
    template: `<div class="marker-container"><ar-marker *ngFor="let marker of markers" [marker]="marker"></ar-marker></div>`,
    styles: [
        'marker-container { display: flex; flex-wrap: 1; }',
        'ar-marker { margin: 30px; display: inline-block; }'
    ]
})
export class MarkerTestComponent implements OnInit, OnDestroy
{
    private markers: Marker[] = [];

    constructor(private markerProvider: MarkerProvider) { }

    ngOnInit() {
        // quick hack to disable global css for main app
        document.getElementsByTagName('html')[0].style.overflow = "auto";

        for (let i = 0; i < 512; i++) {
            this.markers.push(this.markerProvider.createMarker());
        }
    }

    ngOnDestroy() {
        for (let marker of this.markers) {
            this.markerProvider.destroyMarker(marker);
        }
    }
}
