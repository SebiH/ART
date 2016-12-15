import { Component, OnInit, OnDestroy } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';

@Component({
  selector: 'graph-create-form',
  templateUrl: './app/components/graph-create-form/graph-create-form.html',
  styleUrls: ['./app/components/graph-create-form/graph-create-form.css'],
})
export class GraphCreateFormComponent implements OnInit, OnDestroy {

    private dimensions: string[] = [ "Dummy_Calories", "Dummy_Vitamin_C", "Dummy_Happiness", "Dummy_Vitamin_D" ];

    private selectedDimX: string = "";
    private selectedDimY: string = "";

    constructor (private graphProvider: GraphProvider) {}

    ngOnInit() {
    }

    ngOnDestroy() {
    }

    private toggleButtonX(dim: string) {
        this.selectedDimX = dim;
    }

    private toggleButtonY(dim: string) {
        this.selectedDimY = dim;
    }

    private createGraph(): void {
        if (this.selectedDimX.length > 0 && this.selectedDimY.length > 0)
        this.graphProvider.addGraph(this.selectedDimX, this.selectedDimY);
        this.selectedDimX = "";
        this.selectedDimY = "";
    }
}