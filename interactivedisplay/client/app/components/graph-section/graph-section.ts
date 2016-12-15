import { Component, Input, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { Marker, Graph } from '../../models/index';
import { MarkerProvider } from '../../services/index';

@Component({
  selector: 'graph-section',
  templateUrl: './app/components/graph-section/graph-section.html',
  styleUrls: ['./app/components/graph-section/graph-section.css'],
})
export class GraphSectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;
    private marker: Marker;

    constructor (private markerProvider: MarkerProvider) {}

    ngOnInit() {
      this.marker = this.markerProvider.createMarker();
    }

    ngOnDestroy() {
      this.markerProvider.destroyMarker(this.marker);
    }
}
