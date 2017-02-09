import { Component, OnInit, OnDestroy } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';

import * as _ from 'lodash';

@Component({
    selector: 'graph-list',
    templateUrl: './app/components/graph-list/graph-list.html',
    styleUrls: ['./app/components/graph-list/graph-list.css'],
})
export class GraphListComponent implements OnInit, OnDestroy {

    private graphs: Graph[] = [];
    private isActive: boolean = true;

    private isScrolling: boolean = false;
    private interactionCounter: number = 0;

    private listStyle = {
        '-webkit-transform': 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)',
        '-ms-transform': 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)',
        transform: 'translate3d(' + (-this.scrollOffset) + 'px, 0, 0)'
    };

    private _scrollOffset : number = 0;
    private get scrollOffset(): number {
        return this._scrollOffset;
    }
    private set scrollOffset(v: number) {
        this._scrollOffset = v;

        let transform = 'translate3d(' + (-v) + 'px, 0, 0)';
        this.listStyle['-webkit-transform'] = transform;
        this.listStyle['-ms-transform'] = transform;
        this.listStyle.transform = transform;
    }

    constructor(private graphProvider: GraphProvider) {}

    ngOnInit() {
        this.graphProvider.getGraphs()
            .takeWhile(() => this.isActive)
            .subscribe(graphs => { 
                this.graphs = graphs;
                this.applyScrollOffsetLimits();
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

    private handleMoveStart(event: any): void {
        // TODO: multitouch??
        this.isScrolling = true;
    }

    private handleMoveUpdate(event: any): void {
        this.scrollOffset += event.deltaX;

        for (let graph of this.graphs) {
            if (graph.isPickedUp) {
                graph.posOffset += event.deltaX;
            }
        }
    }

    private applyScrollOffsetLimits(): void {
        const minVisiblePercent = 0.9;
        let minVisible = window.innerWidth * minVisiblePercent;
        let totalGraphWidth = _.sumBy(this.graphs, 'width');
        let maxOffset = totalGraphWidth;
        if (this.graphs.length > 0) {
            maxOffset -= _.last(this.graphs).width * (1 - minVisiblePercent);
        }

        this.scrollOffset = Math.max(-minVisible, Math.min(this.scrollOffset, maxOffset));
    }

    private handleMoveEnd(event: any): void {
        this.isScrolling = false;
        this.applyScrollOffsetLimits();
    }


    private onGraphCreation(graph: Graph): void {
        // TODO: adjust scrolloffset so that graph is only slightly in view
        this.graphProvider.setGraphOffset(graph, graph.posOffset + this.scrollOffset);
    }
}
