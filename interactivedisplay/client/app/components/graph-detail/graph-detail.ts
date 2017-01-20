import { Component, OnInit, OnDestroy, Input, EventEmitter, Output } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { GraphDataProvider } from '../../services/index';
import { Graph } from '../../models/index';

@Component({
    selector: 'graph-detail',
    templateUrl: './app/components/graph-detail/graph-detail.html',
    styleUrls: ['./app/components/graph-detail/graph-detail.css'],
})
export class GraphDetailComponent implements OnInit, OnDestroy {

    @Input() graph: Graph;

    @Output() onClose = new EventEmitter();

    private dimensions: string[];
    private dimensionSubscription: Subscription;

    constructor (private graphDataProvider: GraphDataProvider) {}

    ngOnInit() {
        this.dimensionSubscription = this.graphDataProvider.getDimensions()
            .subscribe((dims) => this.dimensions = dims);
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
}
