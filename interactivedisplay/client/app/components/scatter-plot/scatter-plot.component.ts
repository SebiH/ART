import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ChartDimension } from '../../models/index';
import { ChartElement } from './chart-element';
import { ChartPolygon } from './chart-polygon';
import { ChartAxis, AxisType } from './chart-axis';
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
    private chartRoot: ChartElement;
    private xAxis: ChartAxis;
    private yAxis: ChartAxis;
    private chartValues: ChartElement;

    constructor() { }

    ngOnInit() {
        this.initGraph();
    }

    ngOnDestroy() {

    }


    public loadData(dimX: ChartDimension, dimY: ChartDimension): void {
        this.initGraph();

        if (dimX) {
            this.xAxis.setDomain(dimX.domain.min, dimX.domain.max);
        } else {
            this.xAxis.setDomain(0, 1);
        }

        if (dimY) {
            this.yAxis.setDomain(dimY.domain.min, dimY.domain.max);
        } else {
            this.yAxis.setDomain(0, 1);
        }

        // values
        if (this.chartValues) {
            this.chartValues.remove();
        }

        if (dimX && dimY) {
            let data: number[][] = [];

            // assuming dimX data length === dimY data length
            for (let i = 0; i < dimX.data.length; i++) {
                data.push([dimX.data[i], dimY.data[i]]);
            }

            this.chartValues = this.chartRoot.selectAll('dot')
                .data(data)
                .enter().append('circle')
                    .attr('r', 5)
                    .attr('cx', d => this.xAxis.scale(d[0]))
                    .attr('cy', d => this.yAxis.scale(d[1]));
        }
    }

    public createSelectionPolygon(): ChartPolygon {
        let selection = new ChartPolygon();
        selection.init(this.chartRoot);
        return selection;
    }


    private initGraph(): void {
        if (this.chartRoot) {
            // already initialised
            return;
        }

        let d3element = d3.select(this.graphElement.nativeElement);
        d3element.html('');

        let chartSvg = d3element.append('svg')
            .attr('width', this.width + this.margin.left + this.margin.right)
            .attr('height', this.height + this.margin.top + this.margin.bottom);

        this.chartRoot = chartSvg.append('g')
            .attr('transform', 'translate(' + this.margin.left + ',' + this.margin.top + ')');;

        this.xAxis = new ChartAxis(this.chartRoot, AxisType.Horizontal, this.width, this.height);
        this.yAxis = new ChartAxis(this.chartRoot, AxisType.Vertical, this.height);
    }
}