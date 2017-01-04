import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { GraphProvider, GraphDataProvider } from '../../services/index';
import { Graph } from '../../models/index';

@Component({
  selector: 'graph-create-form',
  templateUrl: './app/components/graph-create-form/graph-create-form.html',
  styleUrls: ['./app/components/graph-create-form/graph-create-form.css'],
})
export class GraphCreateFormComponent implements OnInit, OnDestroy {

    private dimensions: string[];

    private selectedDimX: string = "";
    private selectedDimY: string = "";

    private dimSubscription: Subscription;

    constructor (
        private graphProvider: GraphProvider, 
        private graphDataProvider: GraphDataProvider) {}

    ngOnInit() {
        this.dimSubscription = this.graphDataProvider.getDimensions()
            .subscribe(dim => this.dimensions = dim);
    }

    ngOnDestroy() {
        this.dimSubscription.unsubscribe();
    }

    private toggleButtonX(dim: string) {
        this.selectedDimX = dim;
    }

    private toggleButtonY(dim: string) {
        this.selectedDimY = dim;
    }

    private createGraph(): void {
        if (this.selectedDimX.length > 0 && this.selectedDimY.length > 0) {
            let newGraph = this.graphProvider.addGraph();
            newGraph.dimX = this.selectedDimX;
            newGraph.dimY = this.selectedDimY;
            newGraph.updateData();
        }
        this.selectedDimX = "";
        this.selectedDimY = "";
    }
}
