import { Component, OnInit, OnDestroy, ViewChildren } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';
import { GraphSectionComponent } from '../graph-section/graph-section';

import * as _ from 'lodash';

const CARD_WIDTH = 500;

@Component({
    selector: 'graph-container',
    templateUrl: './app/components/graph-container/graph-container.html',
    styleUrls: ['./app/components/graph-container/graph-container.css'],
})
export class GraphContainerComponent implements OnInit, OnDestroy {

    private graphs: Graph[] = [];
    private graphSubscription: Subscription;

    constructor(private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphSubscription = this.graphProvider.getGraphs()
            .subscribe(graphs => this.graphs = _.sortBy(graphs, 'listIndex'));
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
            left: this.getOffset(graph) + 'px'
        };
    }
}
