import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';
import { MoveableDirective } from '../../directives/index';

const CARD_WIDTH = 500;

@Component({
    selector: 'graph-container',
    templateUrl: './app/components/graph-container/graph-container.html',
    styleUrls: ['./app/components/graph-container/graph-container.css']
})
export class GraphContainerComponent implements OnInit, OnDestroy {

    private graphs: Graph[] = [];
    private graphSubscription: Subscription;

    constructor(private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphSubscription = this.graphProvider.getGraphs()
            .subscribe(graphs => this.graphs = graphs);
    }

    ngOnDestroy() {
        this.graphSubscription.unsubscribe();
    }

    private getOffset(graph: Graph) {
        let offset = 0;
        for (let g of this.graphs) {
            if (g.listIndex < graph.listIndex) {
                offset += g.width;
            }
        }

        return offset;
    }

    private getOffsetStyle(graph: Graph) {
        return {
            left: (600 + this.getOffset(graph)) + 'px'
        };
    }
}
