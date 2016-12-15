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
        this.graphProvider.addGraph("todo4", "todo3");
        this.graphProvider.addGraph("todo5", "todo3");
        this.graphProvider.addGraph("todo6", "todo3");
    }

    ngOnDestroy() {

    }

    getStyle(graph: Graph): any {
      let index = this.graphs.indexOf(graph);
      let offset = 600; // offset for the graph creation card
      return {
        "left": offset + (index * 500) + "px"
      };
    }
}
