import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { GraphDataProvider } from '../../services/index';
import { Graph } from '../../models/index';

import * as d3 from 'd3';

@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;

    constructor(private graphDataProvider: GraphDataProvider) {}

    ngOnInit() {

    }

    ngOnDestroy() {

    }
}
