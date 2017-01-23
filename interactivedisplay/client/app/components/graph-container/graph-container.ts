import { Component, OnInit, OnDestroy } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';

import * as _ from 'lodash';

@Component({
    selector: 'graph-container',
    templateUrl: './app/components/graph-container/graph-container.html',
    styleUrls: ['./app/components/graph-container/graph-container.css'],
})
export class GraphContainerComponent implements OnInit, OnDestroy {

    private graphs: Graph[] = [];
    private isActive: boolean = true;
    private scrollEvents: number = 0;

    private scrollOffset: number = 0;
    private selectedGraph: Graph;

    private interactionCounter: number = 0;

    private containerStyle = {
        '-webkit-transform': 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)',
        '-ms-transform': 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)',
        transform: 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)'
    };

    constructor(private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphProvider.getGraphs()
            .takeWhile(() => this.isActive)
            .subscribe(graphs => { 
                this.graphs = graphs;
                this.applyScrollOffsetLimits();
                this.setContainerOffset(this.scrollOffset);
            });
    }

    ngOnDestroy() {
        this.isActive = false;
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

    private setContainerOffset(offset: number): void {
        let transform = 'translate3d(' + (-offset) + 'px, 0, 0)';
        this.containerStyle['-webkit-transform'] = transform;
        this.containerStyle['-ms-transform'] = transform;
        this.containerStyle.transform = transform;
    }

    private getGraphStyle(graph: Graph): any {
        let offset = this.getOffset(graph) + graph.posOffset;
        let transform = 'translate3d(' + offset +'px, 0, 0)';
        let style = {
            'z-index': graph.listIndex,
            '-webkit-transform': transform,
            '-ms-transform': transform,
            transform: transform
        };

        if (graph.isPickedUp) {
            style['z-index'] += 100;
        }

        return style;
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
        this.scrollEvents++;
    }

    private handleMoveUpdate(event: any): void {
        let oldOffset = this.scrollOffset;
        this.scrollOffset += event.deltaX;
        this.applyScrollOffsetLimits();
        this.setContainerOffset(this.scrollOffset);

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
        this.scrollEvents = Math.max(0, this.scrollEvents - 1);
        if (this.scrollEvents == 0) {
            this.applyScrollOffsetLimits();
        }
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
        this.createdGraph.isSelected = true;
        this.createdGraph = null;
    }
}
