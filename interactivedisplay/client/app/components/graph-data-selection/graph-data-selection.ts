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
    private dataSubscription: Subscription;

    private polygonPath;
    private selectionPolygon: Point[] = [];

    private hasTouchDown: boolean = false;
    private touchDownListener: InteractionListener;
    private touchMoveListener: InteractionListener;
    private touchUpListener: InteractionListener;

    constructor(
        private graphDataProvider: GraphDataProvider,
        private interactionManager: InteractionManager) {}

    ngOnInit() {
        this.dataSubscription = Observable
            .zip(
                this.graphDataProvider.getData(this.graph.dimX),
                this.graphDataProvider.getData(this.graph.dimY))
            .subscribe(([dataX, dataY]) => {
                this.displayData(dataX, dataY);
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
        this.dataSubscription.unsubscribe();
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
    }

    private handleTouchMove(ev: InteractionEvent): void {
        if (this.hasTouchDown) {
            if (this.selectionPolygon.length === 0) {
                this.selectionPolygon.push(ev.position);
                this.selectionPolygon.push(ev.position);
            }
            this.selectionPolygon.splice(this.selectionPolygon.length - 2, 0, ev.position);
            this.renderSelectionPolygon();
        }
    }

    private clearSelection(): void {
        // retain object reference for d3
        while (this.selectionPolygon.length > 0) {
            this.selectionPolygon.pop();
        }
        this.renderSelectionPolygon();
    }

    private renderSelectionPolygon(): void {
        if (this.polygonPath) {
            let polygonLine = d3.line()
                .x(d => d.x)
                .y(d => d.y);
            this.polygonPath.attr('d', polygonLine(this.selectionPolygon));
        }
    }

    private displayData(dataX: number[], dataY: number[]) {
        let data = [];
        // assume dataX.length === dataY.length
        for (let i = 0; i < dataX.length; i++) {
            data.push([dataX[i], dataY[i]]);
        }

        let margin = {top: 20, right: 20, bottom: 30, left: 40};
        let width = 960 - margin.left - margin.right;
        let height = 500 - margin.top - margin.bottom;

        let x = d3.scaleLinear()
            .range([0, width])
            .domain([d3.min(data, d => d[0]), d3.max(data, d => d[0])]);
        let y = d3.scaleLinear()
            .range([0, height])
            .domain([d3.min(data, d => d[1]), d3.max(data, d => d[1])]);

        let valueLine = d3.line()
            .x(d => x(d[0]))
            .y(d => y(d[1]));

        let svg = d3.select(this.graphContainer.nativeElement).append('svg')
            .attr('width', width + margin.left + margin.right)
            .attr('height', height + margin.top + margin.bottom)
            .append('g')
            .attr('transform', 'translate(' + margin.left + ',' + margin.top + ')');

        let polygonLine = d3.line()
            .x(d => d.x)
            .y(d => d.y);

        this.polygonPath = svg.append('path')
            .attr('d', polygonLine(this.selectionPolygon))
            .attr('stroke', 'blue')
            .attr('stroke-width', 2)
            .attr('fill', '#00FF00')
            .attr('fill-opacity', '0.4');


        svg.selectAll('dot')
            .data(data)
            .enter().append('circle')
                .attr('r', 5)
                .attr('cx', d => x(d[0]))
                .attr('cy', d => y(d[1]));

        svg.append('g')
            .attr('transform', 'translate(0,' + height + ')')
            .call(d3.axisBottom(x));

        svg.append('g')
            .call(d3.axisLeft(y));
    }
}
