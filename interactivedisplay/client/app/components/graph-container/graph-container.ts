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

    private scrollOffset: number = 0;
    private selectedGraph: Graph;

    private interactionCounter: number = 0;

    constructor(private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphSubscription = this.graphProvider.getGraphs()
            .subscribe(graphs => {
                this.graphs = graphs;
            });
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

    private getContainerStyle(): any {
        return {
            '-webkit-transform': 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)',
            '-ms-transform': 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)',
            transform: 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)'
        };
    }

    private getStyle(graph: Graph): any {
        let style = {
            width: graph.width + 'px',
            background: graph.color,
            'z-index': graph.listIndex,
            '-webkit-transform': 'translate3d(' + (this.getOffset(graph) + graph.posOffset) +'px, 0, 0)',
            '-ms-transform': 'translate3d(' + (this.getOffset(graph) + graph.posOffset) +'px, 0, 0)',
            transform: 'translate3d(' + (this.getOffset(graph) + graph.posOffset) +'px, 0, 0)'
        };

        if (graph.isPickedUp) {
            style['z-index'] += 100;
        }

        return style;
    }

    private deleteGraph(graph: Graph, event: any): void {
        this.graphProvider.removeGraph(graph);
        this.applyScrollOffsetLimits();
    }

    private selectGraph(graph: Graph): void {
        graph.isSelected = true;
        graph.width = 1500;
        graph.updateData();
    }

    private deselectGraph(graph: Graph): void {
        graph.isSelected = false;
        graph.width = 700;
        graph.updateData();
    }

    private moveGraph(graph: Graph, event: any): void {
        if (_.has(event, 'start') && event.start) {
            graph.isPickedUp = true;
            this.interactionCounter++;
        } else if (_.has(event, 'end') && event.end) {
            graph.isPickedUp = false;
            graph.posOffset = 0;
            this.interactionCounter--;
        } else {

            graph.posOffset -= event.delta;

            let sortedGraphs = _.sortBy(this.graphs, 'listIndex');
            let graphIndex = sortedGraphs.indexOf(graph);

            if (graph.posOffset < 0 && graphIndex > 0) {
                let prevGraph = sortedGraphs[graphIndex - 1];
                if (-graph.posOffset > prevGraph.width / 2) {
                    this.graphProvider.moveLeft(graph);
                    graph.posOffset += prevGraph.width;
                }
            } else if (graph.posOffset > 0 && graphIndex < sortedGraphs.length - 1) {
                let nextGraph = sortedGraphs[graphIndex + 1];
                if (graph.posOffset > nextGraph.width / 2) {
                    this.graphProvider.moveRight(graph);
                    graph.posOffset -= nextGraph.width;
                }
            }
        }

    }


    private handleMoveStart(event: any): void {
    }

    private handleMoveUpdate(event: any): void {
        let oldOffset = this.scrollOffset;
        this.scrollOffset += event.deltaX;
        this.applyScrollOffsetLimits();

        let appliedDeltaX = this.scrollOffset - oldOffset;

        for (let graph of this.graphs) {
            if (graph.isPickedUp) {
                graph.posOffset += appliedDeltaX;
            }
        }
    }

    private applyScrollOffsetLimits(): void {
        let maxWidth = _.sumBy(this.graphs, 'width') - window.innerWidth;
        this.scrollOffset = Math.max(0, Math.min(this.scrollOffset, maxWidth));
    }

    private handleMoveEnd(event: any): void {
    }

    private handleMoveDown(graph: Graph): void {
        this.graphProvider.moveLeft(graph);
    }

    private handleMoveUp(graph: Graph): void {
        this.graphProvider.moveRight(graph);
    }


    private createdGraph: Graph;

    private handleCreateStart(event: any) {
        let graph = this.graphProvider.addGraph();
        this.createdGraph = graph;
        this.createdGraph.posOffset += this.scrollOffset;
        this.moveGraph(this.createdGraph, {
            start: true
        });
    }

    private handleCreateUpdate(event: any) {
        this.moveGraph(this.createdGraph, {
            delta: event.deltaX
        });
    }

    private handleCreateEnd(event: any) {
        this.moveGraph(this.createdGraph, {
            end: true
        });
        this.selectGraph(this.createdGraph);
        this.createdGraph = null;
    }
}
