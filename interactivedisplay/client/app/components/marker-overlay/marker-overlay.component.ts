import { Component, OnInit } from '@angular/core';
import { MarkerProvider } from '../../services/index';

@Component({
    selector: 'marker-overlay',
    template: `<div>X</div>`,
    styles: [
    ]
})

export class MarkerOverlayComponent implements OnInit {

    constructor (private markerProvider: MarkerProvider) {
    }

    ngOnInit() {
    }
}
