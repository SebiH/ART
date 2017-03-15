import { Component, AfterViewInit, OnChanges, SimpleChanges, ViewChild, Input } from '@angular/core';
import { ChartDimension } from '../../models/index';
import { ChartDirective, HtmlChartElement, ChartElement } from '../../directives/index';

import { ChartAxis } from './chart-axis';
import { ScatterplotVisualisation } from './scatterplot-visualisation';

import * as d3 from 'd3';
import * as _ from 'lodash';

@Component({
    selector: 'chart-2d',
    template: `<div chart 
                    [width]="width"
                    [height]="height"
                    [margin]="margin">
               </div>`
})
export class Chart2dComponent implements AfterViewInit, OnChanges {

    @Input() dimensionX: ChartDimension = null;
    @Input() dimensionY: ChartDimension = null;
    @Input() width: number = 300;
    @Input() height: number = 900;
    @Input() margin = { top: 50, right: 50, bottom: 100, left: 100 };

    @ViewChild(ChartDirective) chart: ChartDirective;

    private isLoaded = false;

    private dataVisualisation: ScatterplotVisualisation;
    private xAxis: ChartAxis;
    private yAxis: ChartAxis;

    public xScale: any;
    public yScale: any;

    ngAfterViewInit() {
        this.isLoaded = true;
        this.initialize();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (this.isLoaded) {
            this.initialize();
        }
    }

    private initialize(): void {
        if (!this.dataVisualisation) {
            this.dataVisualisation = new ScatterplotVisualisation();
            this.chart.addElement(this.dataVisualisation);
            this.xAxis = new ChartAxis('x');
            this.chart.addElement(this.xAxis);
            this.yAxis = new ChartAxis('y');
            this.chart.addElement(this.yAxis);
        }

        this.xScale = this.getScale(this.dimensionX, 'x');
        this.yScale = this.getScale(this.dimensionY, 'y');
        this.xAxis.setScale(this.xScale);
        this.yAxis.setScale(this.yScale);

        if (this.dimensionX == null || this.dimensionY == null) {
            this.dataVisualisation.clearData();
        } else {
            this.dataVisualisation.loadData(this.dimensionX, this.dimensionY, this.xScale, this.yScale);
        }
    }


    public addElement(chartElement: ChartElement): void {
        this.chart.addElement(chartElement);
    }

    public removeElement(chartElement: ChartElement): void {
        this.chart.removeElement(chartElement);
    }


    private getScale(dim: ChartDimension, type: 'x' | 'y') {
        let range = type === 'x' ? [0, this.width] : [this.height, 0];

        if (dim == null) {
            return d3.scaleLinear().range(range).domain([0, 1]);
        } else { //if (dim.isMetric) {
            return d3.scaleLinear().range(range).domain([dim.domain.min, dim.domain.max]);
        }
        // else {
        //     let domain = <string[]>_.map(dim.mappings, 'name');

        //     // flip domain
        //     if (type === 'y') {
        //         let flipped: string[] = [];
        //         for (let i = 0; i < domain.length; i++) {
        //             flipped[domain.length - 1 - i] = domain[i];
        //         }
        //         domain = flipped;
        //     }

        //     // pad domain
        //     domain.unshift('');
        //     domain.push(' ');
        //     console.log(dim);

        //     return d3.scalePoint().domain(domain).range([0, type === 'x' ? this.width : this.height]);
        // }
    }

    public invert(pos: [number, number]): [number, number] {
        if (this.xScale && this.yScale) {
            let x = 0;
            if (this.xScale.invert) {
                x = this.xScale.invert(pos[0]);
            } else {
                // scalepoint has no inverse
                // TODO..
            }

            let y = 0;
            if (this.yScale.invert) {
                y = this.yScale.invert(pos[1]);
            } else {
                // scalepoint has no inverse
                // TODO..
            }

            return [ x, y ];
        }

        return [0, 0];
    }
}
