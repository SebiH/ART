import { Component, OnInit, OnDestroy, Input, ViewChild, ElementRef } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { Graph, Point } from '../../models/index';
import {
    GraphDataProvider,
    InteractionManager,
    InteractionEvent,
    InteractionListener,
    InteractionEventType
} from '../../services/index';

import * as d3 from 'd3';

@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;
    @ViewChild('graphContainer') private graphContainer: ElementRef;
    private graphSubscription: Subscription;
    private dataSubscription: Subscription;

    private data: Point[];
    private scaleX: d3.ScaleLinear<number, number>;
    private scaleY: d3.ScaleLinear<number, number>;
    private margin = { top: 20, right: 20, bottom: 30, left: 40 };

    private polygonPath;

    private hasTouchDown: boolean = false;
    private touchDownListener: InteractionListener;
    private touchMoveListener: InteractionListener;
    private touchUpListener: InteractionListener;

    constructor(
        private graphDataProvider: GraphDataProvider,
        private interactionManager: InteractionManager) {}

    ngOnInit() {
        this.graphSubscription = this.graph.onDataUpdate
            .subscribe(() => {
                if (this.dataSubscription) {
                    this.dataSubscription.unsubscribe();
                    this.dataSubscription = null;
                }

                if (this.graph.dimX.length > 0 && this.graph.dimY.length > 0) {
                    this.dataSubscription = Observable
                        .zip(
                            this.graphDataProvider.getData(this.graph.dimX),
                            this.graphDataProvider.getData(this.graph.dimY))
                        .subscribe(([dataX, dataY]) => {
                            this.displayData(dataX, dataY);
                            this.renderSelectionPolygon();
                            this.highlightData();
                        });
                }
            });

        this.touchDownListener = {
            type: InteractionEventType.TouchDown,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleTouchDown(ev); }
        };
        this.touchMoveListener = {
            type: InteractionEventType.TouchMove,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleTouchMove(ev); }
        };
        this.touchUpListener = {
            type: InteractionEventType.TouchUp,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleTouchUp(ev); }
        };
        this.interactionManager.on(this.touchDownListener);
        this.interactionManager.on(this.touchMoveListener);
        this.interactionManager.on(this.touchUpListener);
    }

    ngOnDestroy() {
        this.graphSubscription.unsubscribe();
        if (this.dataSubscription) {
            this.dataSubscription.unsubscribe();
        }

        this.interactionManager.off(this.touchDownListener);
        this.interactionManager.off(this.touchMoveListener);
        this.interactionManager.off(this.touchUpListener);
    }


    private handleTouchDown(ev: InteractionEvent): void {
        this.hasTouchDown = true;
        this.clearSelection();
    }

    private handleTouchUp(ev: InteractionEvent): void {
        this.hasTouchDown = false;
        this.highlightData();
    }

    private handleTouchMove(ev: InteractionEvent): void {
        if (this.hasTouchDown) {
            let globalPosition = this.graphContainer.nativeElement.getBoundingClientRect();
            let posOffset = new Point(globalPosition.left + this.margin.left,
                globalPosition.top + this.margin.top);

            this.graph.selectionPolygon.push(Point.sub(ev.position, posOffset));
            this.renderSelectionPolygon();
            this.highlightData();
        }
    }

    private clearSelection(): void {
        while (this.graph.selectionPolygon.length > 0) {
            this.graph.selectionPolygon.pop();
        }
        this.renderSelectionPolygon();
        this.highlightData();
    }

    private renderSelectionPolygon(): void {
        if (this.polygonPath) {
            let polygonLine = d3.line<Point>()
                .curve(d3.curveBasisClosed)
                .x(d => d.x)
                .y(d => d.y);
            this.polygonPath.attr('d', polygonLine(this.graph.selectionPolygon));
        }
    }

    private highlightData(): void {
        if (this.data) {
            while (this.graph.selectedDataIndices.length > 0) {
                this.graph.selectedDataIndices.pop();
            }

            if (this.graph.selectionPolygon.length > 0) {
                let topLeft = new Point(this.graph.selectionPolygon[0].x, this.graph.selectionPolygon[0].y);
                let bottomRight = new Point(this.graph.selectionPolygon[0].x, this.graph.selectionPolygon[0].y);

                for (let p of this.graph.selectionPolygon) {
                    topLeft.x = Math.min(topLeft.x, p.x);
                    topLeft.y = Math.min(topLeft.y, p.y);
                    bottomRight.x = Math.max(bottomRight.x, p.x);
                    bottomRight.y = Math.max(bottomRight.y, p.y);
                }

                for (let index = 0; index < this.data.length; index++) {
                    let datum = new Point(this.scaleX(this.data[index].x), this.scaleY(this.data[index].y));
                    if (datum.isInPolygon(this.graph.selectionPolygon, [topLeft, bottomRight])) {
                        this.graph.selectedDataIndices.push(index);
                    }
                }

            }

            this.graph.updateData();

            // highlight data
            d3.select(this.graphContainer.nativeElement)
                .selectAll('circle')
                .filter((d, i) => this.graph.selectedDataIndices.indexOf(i) > -1)
                .style('fill', 'red');

            // remove highlight from other data
            d3.select(this.graphContainer.nativeElement)
                .selectAll('circle')
                .filter((d, i) => this.graph.selectedDataIndices.indexOf(i) == -1)
                .style('fill', 'black');
        }
    }

    private displayData(dataX: number[], dataY: number[]) {
        this.data = [];
        // assume dataX.length === dataY.length
        for (let i = 0; i < dataX.length; i++) {
            this.data.push(new Point(dataX[i], dataY[i]));
        }

        let width = 960 - this.margin.left - this.margin.right;
        let height = 500 - this.margin.top - this.margin.bottom;

        this.scaleX = d3.scaleLinear()
            .range([0, width])
            .domain([d3.min(this.data, d => d.x), d3.max(this.data, d => d.x)]);
        this.scaleY = d3.scaleLinear()
            .range([0, height])
            .domain([d3.min(this.data, d => d.y), d3.max(this.data, d => d.y)]);

        let valueLine = d3.line<Point>()
            .x(d => this.scaleX(d.x))
            .y(d => this.scaleY(d.y));

        let svg = d3.select(this.graphContainer.nativeElement).append('svg')
            .attr('width', width + this.margin.left + this.margin.right)
            .attr('height', height + this.margin.top + this.margin.bottom)
            .append('g')
            .attr('transform', 'translate(' + this.margin.left + ',' + this.margin.top + ')');

        this.polygonPath = svg.append('path')
            .attr('stroke', 'blue')
            .attr('stroke-width', 2)
            .attr('fill', '#00FF00')
            .attr('fill-opacity', '0.4');


        svg.selectAll('dot')
            .data(this.data)
            .enter().append('circle')
                .attr('r', 5)
                .attr('cx', d => this.scaleX(d.x))
                .attr('cy', d => this.scaleY(d.y));

        svg.append('g')
            .attr('transform', 'translate(0,' + height + ')')
            .call(d3.axisBottom(this.scaleX));

        svg.append('g')
            .call(d3.axisLeft(this.scaleY));
    }
}