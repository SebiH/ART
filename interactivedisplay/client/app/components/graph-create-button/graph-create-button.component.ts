import { Component, Output, EventEmitter } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';

@Component({
    selector: 'graph-create-button',
    templateUrl: './app/components/graph-create-button/graph-create-button.html',
    styleUrls: ['./app/components/graph-create-button/graph-create-button.css'],
})
export class GraphCreateButtonComponent {

    @Output()
    private graphCreation = new EventEmitter();

    private createdGraph: Graph;
    private lastGraphCreation: number = 0;

    constructor(private graphProvider: GraphProvider) {}

    private handleCreateStart(event: any): void {
        this.graphProvider.getGraphs()
            .first()
            .subscribe((graphs) => {

                let currentTime = new Date().getTime();

                // only allow one graph created at a time, with timeout
                if (!graphs.find((g) => g.isNewlyCreated) &&  currentTime - this.lastGraphCreation > 1500) {
                    this.lastGraphCreation = currentTime;
                    this.graphProvider.selectGraph(null);
                    let graph = this.graphProvider.addGraph();
                    this.createdGraph = graph;
                    this.createdGraph.isPickedUp = true;
                    this.graphCreation.emit(this.createdGraph);
                }
            });

    }

    private handleCreateUpdate(event: any): void {
        if (this.createdGraph) {
            this.createdGraph.posOffset -= event.deltaX;
        }
    }

    private handleCreateEnd(event: any): void {
        if (this.createdGraph) {
            this.createdGraph.isPickedUp = false;
            this.createdGraph.isNewlyCreated = false;
            this.createdGraph.posOffset = 0;
            this.graphProvider.selectGraph(this.createdGraph);
            this.createdGraph = null;
        }
    }
}
