import { Component, AfterViewInit, OnDestroy, Input } from '@angular/core';
import { Graph, Filter, DetailFilter } from '../../models/index';

@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent {
    @Input() graph: Graph;

    @Input() public width = 600;
    @Input() public height = 600;
    @Input() public margin = { top: 50, right: 50, bottom: 200, left: 200 }; 
}
