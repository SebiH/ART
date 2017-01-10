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

    private getStyle(graph: Graph): any {
        let style = {
            width: graph.width + 'px',
            'z-index': graph.listIndex,
            transform: 'translate3d(' + (this.getOffset(graph) + graph.posOffset) +'px, 0, 0)'
        };

        if (graph.isPickedUp) {
            style['z-index'] += 100;
        }

        return style;
    }

    private deleteGraph(graph: Graph, event: any): void {
        this.graphProvider.removeGraph(graph);
    }

    private selectGraph(graph: Graph, event: any): void {
        // TODO.
    }

    private moveGraph(graph: Graph, event: any): void {
        if (_.has(event, 'start') && event.start) {
            graph.isPickedUp = true;
        } else if (_.has(event, 'end') && event.end) {
            graph.isPickedUp = false;
            graph.posOffset = 0;
        } else {

            graph.posOffset -= event.delta;

            let sortedGraphs = _.sortBy(this.graphs, 'listIndex');
            let graphIndex = sortedGraphs.indexOf(graph);

            if (graph.posOffset < 0 && graphIndex > 0) {
                let prevGraph = sortedGraphs[graphIndex - 1];
                if (-graph.posOffset > prevGraph.width / 2) {
                    prevGraph.listIndex++;
                    graph.listIndex--;
                    graph.posOffset += prevGraph.width;
                    graph.updateData();
                    prevGraph.updateData();
                }
            } else if (graph.posOffset > 0 && graphIndex < sortedGraphs.length - 1) {
                let nextGraph = sortedGraphs[graphIndex + 1];
                if (graph.posOffset > nextGraph.width / 2) {
                    nextGraph.listIndex--;
                    graph.listIndex++;
                    graph.posOffset -= nextGraph.width;
                    graph.updateData();
                    nextGraph.updateData();
                }
            }
        }

    }
}
