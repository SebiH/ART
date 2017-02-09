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

    constructor(private graphProvider: GraphProvider) {}

    private handleCreateStart(event: any): void {
        let graph = this.graphProvider.addGraph();
        this.createdGraph = graph;
        this.createdGraph.isPickedUp = true;
        this.graphCreation.emit(this.createdGraph);
    }

    private handleCreateUpdate(event: any): void {
        let newOffset = this.createdGraph.posOffset -= event.deltaX;
        this.graphProvider.setGraphOffset(this.createdGraph, newOffset);
    }

    private handleCreateEnd(event: any): void {
        this.createdGraph.isPickedUp = false;
        this.graphProvider.setGraphOffset(this.createdGraph, 0);
        this.createdGraph.isSelected = true;
        this.createdGraph.updateData(['isSelected']);
        this.createdGraph = null;
    }
}
