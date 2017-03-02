import { Component, AfterViewInit, OnChanges, SimpleChanges, ViewChild, Input, ElementRef } from '@angular/core';
import { ChartDimension } from '../../models/index';
import { ChartDirective } from '../../directives/index';

import { ChartVisualisation1d } from './chart-visualisation-1d';
import { BarVisualisation1d } from './bar-visualisation-1d';

@Component({
    selector: 'chart-1d',
    template: `<div chart 
                    [width]="width - margin.left - margin.right"
                    [height]="height - margin.top - margin.bottom"
                    [margin]="margin">
               </div>`
})
export class Chart1dComponent implements AfterViewInit, OnChanges {

    @Input() dimension: ChartDimension = null;
    @Input() width: number = 300;
    @Input() height: number = 900;

    @ViewChild(ChartDirective) chart: ChartDirective;

    private margin = { top: 0, right: 0, bottom: 0, left: 0 };
    private dataVisualisation: ChartVisualisation1d = null;

    ngAfterViewInit() {
        this.initialize();
    }

    ngOnChanges(changes: SimpleChanges) {
        this.initialize();
    }

    private initialize(): void {
        if (this.dimension === null) {
            this.clear();
        } else {
            if (this.dimension.isMetric) {
                this.drawLineChart();
            } else {
                this.drawBarChart();
            }
        }
    }


    private clear() {
        if (this.dataVisualisation !== null) {
            this.chart.removeElement(this.dataVisualisation);
            this.dataVisualisation = null;
        }
    }

    private drawBarChart() {
        if (this.dataVisualisation === null || this.dataVisualisation.dimension !== this.dimension) {
            this.clear();
            this.dataVisualisation = new BarVisualisation1d(this.dimension);
            this.chart.addElement(this.dataVisualisation);
        }
    }

    private drawLineChart() {
        // TODO?
        // this.drawBarChart();
    }
}