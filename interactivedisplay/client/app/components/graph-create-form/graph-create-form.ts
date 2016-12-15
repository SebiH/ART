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

    private selectedDimX: string;
    private selectedDimY: string;

    constructor (private graphProvider: GraphProvider) {}

    ngOnInit() {
    }

    ngOnDestroy() {
    }

    toggleButtonX(dim: string) {
        this.selectedDimX = dim;
    }

    toggleButtonY(dim: string) {
        this.selectedDimY = dim;
    }
}
