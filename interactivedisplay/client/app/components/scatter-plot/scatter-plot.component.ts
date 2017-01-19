import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ChartDimension } from '../../models/index';
import { PlotElement } from './plot-element';
import { PlotSelection } from './plot-selection';
import * as d3 from 'd3';

@Component({
    selector: 'scatter-plot',
    templateUrl: './app/components/scatter-plot/scatter-plot.html',
    styleUrls: [ './app/components/scatter-plot/scatter-plot.css' ]
})
export class ScatterPlotComponent implements OnInit, OnDestroy {

    public width = 960;
    public height = 500;
    public margin = { top: 20, right: 20, bottom: 30, left: 40 };

    @ViewChild('graph')
    private graphElement: ElementRef;
    private plotRoot: PlotElement;
    private xAxis: PlotElement;
    private yAxis: PlotElement;

    constructor() { }

    ngOnInit() {
        this.initGraph();
    }

    ngOnDestroy() {

    }


    public loadData(dimX: ChartDimension, dimY: ChartDimension): void {
        // let data: Point[] = [];

        // this.plotRoot.selectAll('dot')
        //     .data(data)
        //     .enter().append('circle')
        //         .attr('r', 5)
        //         .attr('cx', d => this.scaleX(d.x))
        //         .attr('cy', d => this.scaleY(d.y));

        // this.plotRoot.append('g')
        //     .attr('transform', 'translate(0,' + height + ')')
        //     .call(d3.axisBottom(this.scaleX));

        // this.plotRoot.append('g')
        //     .call(d3.axisLeft(this.scaleY));

        // svg.selectAll('dot')
        //     .data(this.data)
        //     .enter().append('circle')
        //         .attr('r', 5)
        //         .attr('cx', d => this.scaleX(d.x))
        //         .attr('cy', d => this.scaleY(d.y));
    }

    public createSelectionPolygon(): PlotSelection {
        let selection = new PlotSelection();
        selection.init(this.plotRoot);
        return selection;
    }

    private initGraph(): void {
        let d3element = d3.select(this.graphElement.nativeElement);
        d3element.html('');

        let plotSvg = d3element.append('svg')
            .attr('width', this.width + this.margin.left + this.margin.right)
            .attr('height', this.height + this.margin.top + this.margin.bottom);

        this.plotRoot = plotSvg.append('g')
            .attr('transform', 'translate(' + this.margin.left + ',' + this.margin.top + ')');;

        // draw empty axis 
        let scaleX = d3.scaleLinear().range([0, this.width]).domain([0, 1]);
        this.xAxis = this.plotRoot.append('g')
            .attr('transform', 'translate(0,' + this.height + ')')
            .call(d3.axisBottom(scaleX));

        let scaleY = d3.scaleLinear().range([0, this.height]).domain([0, 1]);
        this.yAxis = this.plotRoot.append('g')
            .call(d3.axisLeft(scaleY));
    }
}