import { Component, OnInit, OnDestroy } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';

@Component({
  selector: 'graph-container',
  templateUrl: './app/components/graph-container/graph-container.html',
  styleUrls: ['./app/components/graph-container/graph-container.css'],
})
export class GraphContainerComponent implements OnInit, OnDestroy {
    private graphs: Graph[];

    constructor (private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphs = this.graphProvider.getGraphs();
        this.graphProvider.addGraph("todo1", "todo2");
        this.graphProvider.addGraph("todo1", "todo3");
        this.graphProvider.addGraph("todo2", "todo3");
    }

    ngOnDestroy() {

    }
}
