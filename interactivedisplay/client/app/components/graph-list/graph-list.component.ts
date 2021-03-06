import { Component, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Observable, Observer } from 'rxjs/Rx';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';
import { GraphListItemComponent } from '../graph-list-item/graph-list-item.component';

import * as _ from 'lodash';

@Component({
    selector: 'graph-list',
    templateUrl: './app/components/graph-list/graph-list.html',
    styleUrls: ['./app/components/graph-list/graph-list.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphListComponent implements OnInit, OnDestroy {

    @ViewChildren(GraphListItemComponent) listItems: QueryList<GraphListItemComponent>;

    private graphs: Graph[] = [];
    private newGraphs: Graph[] = [];
    private isActive: boolean = true;

    private selectedGraph: Graph = null;
    private isScrolling: boolean = false;
    private hasInertia: boolean = false;
    private lastMovement: number = 0;
    private interactionCounter: number = 0;

    private listStyle = {
        left: -this.scrollOffset
    };

    private _scrollOffset : number = 0;
    private get scrollOffset(): number {
        return this._scrollOffset;
    }
    private set scrollOffset(v: number) {
        this._scrollOffset = v;

        this.listStyle.left = -this.scrollOffset;
        this.changeDetector.markForCheck();
    }

    constructor(private graphProvider: GraphProvider, private changeDetector: ChangeDetectorRef) {}

    ngOnInit() {
        Observable.timer(0, 10)
            .takeWhile(() => this.isActive)
            .subscribe(this.checkForChanges.bind(this));

        this.graphProvider.getGraphs()
            .takeWhile(() => this.isActive)
            .subscribe(graphs => {
                this.graphs = graphs;
                this.applyScrollOffsetLimits();
            });

        this.graphProvider.onGraphSelectionChanged()
            .takeWhile(() => this.isActive)
            .subscribe(selectedGraph => {
                this.newGraphs = [];

                let prevSelectedGraph = this.selectedGraph;
                if (selectedGraph) {
                    this.focusGraph(selectedGraph);
                } else if (prevSelectedGraph) {
                    this.focusGraph(prevSelectedGraph);
                }
                this.selectedGraph = selectedGraph;
            });
    }

    ngOnDestroy() {
        this.isActive = false;
    }


    private checkForChanges(): void {
        let listItems = this.listItems.toArray();
        for (let item of listItems) {
            let pos = item.getPosition();
            let currentPosition = pos.left + pos.width / 2;

            if (item.graph.absolutePos == undefined || item.graph.absolutePos !== currentPosition) {
                item.graph.absolutePos = currentPosition;
            }
        }
    }

    private getOffset(graph: Graph) {
        let offset = 0;
        for (let g of this.graphs) {
            if (g.listIndex < graph.listIndex && !g.isNewlyCreated) {
                offset += g.width;
            }
        }

        return offset;
    }

    private handleMoveStart(event: any): void {
        // TODO: multitouch??
        if (this.selectedGraph === null) {
            this.isScrolling = true;
            this.lastMovement = 0;
        }
    }

    private handleMoveUpdate(event: any): void {
        if (this.selectedGraph === null) {
            let prevScrollOffset = this.scrollOffset;
            this.scrollOffset += event.deltaX;
            this.applyScrollOffsetLimits();
            let scrollOffsetDelta = this.scrollOffset - prevScrollOffset;
            this.lastMovement = scrollOffsetDelta;

            for (let graph of this.graphs) {
                if (graph.isPickedUp) {
                    graph.posOffset += scrollOffsetDelta;
                }
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
        if (this.selectedGraph === null) {
            this.isScrolling = false;
            this.applyScrollOffsetLimits();
            this.applyInertia(this.lastMovement);
        }
    }

    private applyInertia(force: number): void {
        if (Math.abs(force) < 1 || this.isScrolling || this.selectedGraph) {
            this.hasInertia = false;
            this.applyScrollOffsetLimits();
            return;
        }

        this.hasInertia = true;
        let prevScrollOffset = this.scrollOffset;
        this.scrollOffset += force;
        this.applyScrollOffsetLimits();
        let scrollOffsetDelta = this.scrollOffset - prevScrollOffset;

        for (let graph of this.graphs) {
            if (graph.isPickedUp) {
                graph.posOffset += scrollOffsetDelta;
            }
        }

        setTimeout(() => this.applyInertia(force * 0.95), 10);
    }


    private onGraphCreation(graph: Graph, pos: 'left' | 'right'): void {
        if (pos === 'left') {
            graph.posOffset = graph.posOffset + this.scrollOffset - graph.width * 0.8;
        }

        if (pos == 'right') {
            graph.posOffset = graph.posOffset + window.innerWidth + this.scrollOffset - graph.width * 0.8;
        }

        this.newGraphs.push(graph);
    }

    private focusGraph(graph: Graph): void {
        this.scrollOffset = this.getOffset(graph) + graph.width / 2 - window.innerWidth / 2;
    }

    private getIndicatorStyle(graph: Graph): any {
        let offset = 0;
        for (let g of this.graphs) {
            if (g.absolutePos < graph.absolutePos && !g.isNewlyCreated) {
                offset += g.width;
            }
        }

        let transform = 'translate3d(' + offset + 'px, 0, 0)';
        return {
            '-webkit-transform': transform,
            '-ms-transform': transform,
            transform: transform
        };
    }

    private selectNext(): void {
        let nextGraph: Graph = null;

        for (let graph of this.graphs) {
            let isNext = graph.listIndex > this.selectedGraph.listIndex;
            let isImmediateNext = (nextGraph == null || graph.listIndex < nextGraph.listIndex);

            if (isNext && isImmediateNext) {
                nextGraph = graph;
            }
        }

        this.graphProvider.selectGraph(nextGraph);
    }

    private selectPrev(): void {
        let prevGraph: Graph = null;

        for (let graph of this.graphs) {
            let isPrev = graph.listIndex < this.selectedGraph.listIndex;
            let isImmediatePrev = (prevGraph == null || graph.listIndex > prevGraph.listIndex);

            if (isPrev && isImmediatePrev) {
                prevGraph = graph;
            }
        }

        this.graphProvider.selectGraph(prevGraph);
    }
}
