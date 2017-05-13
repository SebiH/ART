import { Component, OnInit, OnDestroy } from '@angular/core';
import { MarkerProvider } from '../../services/index';
import { Marker } from '../../models/index';

@Component({
    selector: 'marker-overlay',
    template: `
<div class="overlay">
    <div class="marker-row" *ngFor="let row of rows">
        <div class="marker" *ngFor="let col of columns">
            <ar-marker [marker]="markers[row * maxColumns + col]"></ar-marker>
        </div>
    </div>
</div>`,
    styles: [
        `.overlay {
            width: 100%; height: 100%;
            background: white;
            display: flex;
            align-items: center;
            justify-content: flex-end;
            flex-direction: column;
        }`,
        '.marker { margin: 30px; display: inline-block; }'
    ]
})
export class MarkerOverlayComponent implements OnInit, OnDestroy {

    private maxRows: number = 4;
    private rows: number[] = [];

    private maxColumns: number = 4;
    private columns: number[] = [];

    private markers: Marker[] = [];

    constructor (private markerProvider: MarkerProvider) {
        for (let i = 0; i < this.maxRows; i++) {
            this.rows.push(i);
        }
        for (let i = 0; i < this.maxColumns; i++) {
            this.columns.push(i);
        }
    }

    ngOnInit() {
        for (let i = 0; i < this.maxRows * this.maxColumns; i++) {
            this.markers.push(this.markerProvider.createMarker());
        }
    }

    ngOnDestroy() {
        for (let marker of this.markers) {
            this.markerProvider.destroyMarker(marker);
        }
    }
}
