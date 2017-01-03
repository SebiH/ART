import { Component, OnInit } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';
import { MoveableDirective } from '../../directives/index';

const CARD_WIDTH = 500;

@Component({
  selector: 'graph-container',
  templateUrl: './app/components/graph-container/graph-container.html',
  styleUrls: ['./app/components/graph-container/graph-container.css']
})
export class GraphContainerComponent implements OnInit {
    private graphs: Graph[];

    private draggedGraphId: number = -1;
    private totalMoveDelta: number = 0;

    constructor (private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphs = this.graphProvider.getGraphs();
    }

    private getStyle(graph: Graph): any {
      let index = this.graphs.indexOf(graph);
      let offset = 600; // offset for the graph creation card

      if (this.draggedGraphId == graph.id) {
        offset -= this.totalMoveDelta;
      }

      return {
        "left": offset + (index * CARD_WIDTH) + "px"
      };
    }

    private handleMoveStart(graph: Graph) {
      this.draggedGraphId = graph.id;
    }

    private handleMoveUpdate(graph: Graph, ev: any) {
      this.totalMoveDelta += ev.deltaX;

      if (Math.abs(this.totalMoveDelta) > CARD_WIDTH) {
        let moveIndexBy = Math.floor(Math.abs(this.totalMoveDelta / CARD_WIDTH));

        if (this.totalMoveDelta < 0) {
          for (let i = 0; i < moveIndexBy; i++) {
            this.graphProvider.moveRight(graph);
          }
        } else {
          for (let i = 0; i < moveIndexBy; i++) {
            this.graphProvider.moveLeft(graph);
          }
        }

        this.totalMoveDelta = this.totalMoveDelta % CARD_WIDTH;
      }
    }

    private handleMoveEnd() {
      this.draggedGraphId = -1;
      this.totalMoveDelta = 0;
    }

    private removeGraph(graph: Graph) {
      this.graphProvider.removeGraph(graph);
    }
}
