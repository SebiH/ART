import { Component, OnInit, OnDestroy, Input, EventEmitter, Output, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { GraphDataProvider } from '../../services/index';
import { Graph, Point } from '../../models/index';
import { GraphDataSelectionComponent } from '../graph-data-selection/graph-data-selection';

@Component({
    selector: 'graph-detail',
    templateUrl: './app/components/graph-detail/graph-detail.html',
    styleUrls: ['./app/components/graph-detail/graph-detail.css'],
})
export class GraphDetailComponent implements OnInit, OnDestroy {

    @Input()
    graph: Graph;

    @Output()
    onClose = new EventEmitter();

    @ViewChild('selection')
    graphDataSelection: GraphDataSelectionComponent;

    private dimensions: string[];
    private dimensionSubscription: Subscription;

    private unalteredGraph: any;

    constructor (private graphDataProvider: GraphDataProvider) {}

    ngOnInit() {
        this.dimensionSubscription = this.graphDataProvider.getDimensions()
            .subscribe((dims) => this.dimensions = dims);
        this.unalteredGraph = this.graph.toJson();
    }

    ngOnDestroy() {
        this.dimensionSubscription.unsubscribe();
    }

    private close(): void {
        this.onClose.emit();
    }

    private assignDimX(dim: string): void {
        this.graph.dimX = dim;
        this.graph.updateData(['dimX']);
    }

    private assignDimY(dim: string): void {
        this.graph.dimY = dim;
        this.graph.updateData(['dimY']);
    }

    private confirm() {
        this.close();
    }

    private discard() {
        this.graph.color = this.unalteredGraph.color;
        this.graph.dimX = this.unalteredGraph.dimX;
        this.graph.dimY = this.unalteredGraph.dimY;
        this.graph.selectionPolygons = <Point[][]>Array.from(this.unalteredGraph.selectionPolygons);
        this.graphDataSelection.reloadSelection();
        this.graph.selectedDataIndices = <number[]>Array.from(this.unalteredGraph.selectedData);
        this.graph.updateData(['color', 'dimX', 'dimY', 'selectedDataIndices', 'selectionPolygons']);
    }
}
