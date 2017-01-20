import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ChartDimension } from '../../models/index';
import { ChartElement } from './chart-element';
import { ChartPolygon } from './chart-polygon';
import { ChartAxis, AxisType } from './chart-axis';
import { ChartData } from './chart-data';
import * as d3 from 'd3';

@Component({
    selector: 'scatter-plot',
    templateUrl: './app/components/scatter-plot/scatter-plot.html',
    styleUrls: [ './app/components/scatter-plot/scatter-plot.css' ]
})
export class ScatterPlotComponent implements OnInit, OnDestroy {

    public width = 960;
    public height = 500;
    public margin = { top: 20, right: 20, bottom: 90, left: 90 };

    public data: number[][] = [];

    @ViewChild('graph')
    private graphElement: ElementRef;
    private chartRoot: ChartElement;
    private polygonRoot: ChartElement;
    private xAxis: ChartAxis;
    private yAxis: ChartAxis;
    private chartValues: ChartData;

    constructor() { }

    ngOnInit() {
        this.initGraph();
    }

    ngOnDestroy() {

    }

    public getValues(): ChartData {
        return this.chartValues;
    }

    public loadData(dimX: ChartDimension, dimY: ChartDimension): void {
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
        if (dimX && dimY) {
            this.data = [];

            // assuming dimX data length === dimY data length
            for (let i = 0; i < dimX.data.length; i++) {
                let x = this.xAxis.scale(dimX.data[i]);
                let y = this.yAxis.scale(dimY.data[i]);
                this.data.push([x, y]);
            }

            this.chartValues.setData(this.data);
        } else {
            this.chartValues.clearData();
        }
    }

    public createPolygon(): ChartPolygon {
        let polygon = new ChartPolygon(this.polygonRoot);
        return polygon;
    }


    private initGraph(): void {
        let d3element = d3.select(this.graphElement.nativeElement);
        d3element.html('');

        let chartSvg = d3element.append('svg')
            .attr('width', this.width + this.margin.left + this.margin.right)
            .attr('height', this.height + this.margin.top + this.margin.bottom);

        this.chartRoot = chartSvg.append('g')
            .attr('transform', 'translate(' + this.margin.left + ',' + this.margin.top + ')');;

        this.xAxis = new ChartAxis(this.chartRoot, AxisType.Horizontal, this.width, this.height);
        this.yAxis = new ChartAxis(this.chartRoot, AxisType.Vertical, this.height);
        this.polygonRoot = this.chartRoot.append('g').attr('id', 'polygons');
        this.chartValues = new ChartData(this.chartRoot);
    }
}