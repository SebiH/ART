import { Component, OnInit, OnDestroy, Input, ViewChild, ElementRef } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { GraphDataProvider } from '../../services/index';
import { Graph, Point } from '../../models/index';

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

    private isSelecting: boolean = true;
    private isDeselecting: boolean = false;

    constructor(private graphDataProvider: GraphDataProvider) {}

    ngOnInit() {
        this.dataSubscription = Observable
            .zip(
                this.graphDataProvider.getData(this.graph.dimX),
                this.graphDataProvider.getData(this.graph.dimY))
            .subscribe(([dataX, dataY]) => {
                this.displayData(dataX, dataY);
            });
    }

    ngOnDestroy() {
        this.dataSubscription.unsubscribe();
    }

    private startSelect(): void {
        this.isSelecting = true;
        this.isDeselecting = false;
    }

    private startDeselect(): void {
        this.isDeselecting = true;
        this.isSelecting = false;
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
