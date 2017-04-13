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

    private background: HtmlChartElement;
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
        this.xAxis.setScale(this.getScaleHack(this.dimensionX, 'x'));

        this.yScale = this.getScale(this.dimensionY, 'y');
        this.yAxis.setScale(this.getScaleHack(this.dimensionY, 'y'));

        if (this.dimensionX == null || this.dimensionY == null) {
            this.dataVisualisation.clearData();
        } else {
            this.dataVisualisation.loadData(this.dimensionX, this.dimensionY, this.xScale, this.yScale);
        }
    }

    public addElement(chartElement: ChartElement): void {
        this.chart.addBackgroundElement(chartElement);
    }

    public removeElement(chartElement: ChartElement): void {
        this.chart.removeElement(chartElement);
    }


    private getScale(dim: ChartDimension, type: 'x' | 'y') {
        let range = type === 'x' ? [0, this.width] : [this.height, 0];

        if (dim == null) {
            return d3.scaleLinear().range(range).domain([0, 1]);
        } else if (dim.isMetric) {
            return d3.scaleLinear().range(range).domain([dim.domain.min, dim.domain.max]);
        } else {
            let domain = <number[]>_.map(dim.mappings, 'value');

            // flip domain
            // if (type === 'y') {
            //     let flipped: number[] = [];
            //     for (let i = 0; i < domain.length; i++) {
            //         flipped[domain.length - 1 - i] = domain[i];
            //     }
            //     domain = flipped;
            // }

            // pad domain
            let min = _.min(domain) - 1;
            let max = _.max(domain) + 1;

            let adjustedDomain = type === 'y' ? [max, min] : [min, max];

            return d3.scaleLinear().domain(adjustedDomain).range([0, type === 'x' ? this.width : this.height]);
        }
    }

    private getScaleHack(dim: ChartDimension, type: 'x' | 'y') {
        let range = type === 'x' ? [0, this.width] : [this.height, 0];

        if (dim == null) {
            return d3.scaleLinear().range(range).domain([0, 1]);
        } else if (dim.isMetric) {
            if (dim.isTimeBased) {
                let timeTicks: string[] = [];

                for (let tick of dim.ticks) {
                    if (dim.domain.min <= tick && tick <= dim.domain.max) {
                        let tickDate = new Date(tick * 1000);
                        let tickName = this.formatDate(tickDate, dim.timeFormat, true);
                        if (type == 'x') {
                            timeTicks.push(tickName);
                        } else {
                            timeTicks.unshift(tickName);
                        }
                    }
                }

                return d3.scalePoint().domain(timeTicks).range([0, type === 'x' ? this.width : this.height])
            } else {
                return d3.scaleLinear().range(range).domain([dim.domain.min, dim.domain.max]);
            }
        } else {
            let domain = <string[]>_.map(dim.mappings, 'name');

            // flip domain
            if (type === 'y') {
                let flipped: string[] = [];
                for (let i = 0; i < domain.length; i++) {
                    flipped[domain.length - 1 - i] = domain[i];
                }
                domain = flipped;
            }

            // pad domain
            domain.unshift('');
            domain.push(' ');

            return d3.scalePoint().domain(domain).range([0, type === 'x' ? this.width : this.height]);
        }
    }

    private ii(i, len?) {
        let s = i + "";
        len = len || 2;
        while (s.length < len) s = "0" + s;
        return s;
    }

    // adapted from http://stackoverflow.com/a/14638191/4090817
    private formatDate(date, format, utc): string {
        let MMMM = ["\x00", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        let MMM = ["\x01", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        let dddd = ["\x02", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        let ddd = ["\x03", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];


        let y = utc ? date.getUTCFullYear() : date.getFullYear();
        format = format.replace(/(^|[^\\])yyyy+/g, "$1" + y);
        format = format.replace(/(^|[^\\])yy/g, "$1" + y.toString().substr(2, 2));
        format = format.replace(/(^|[^\\])y/g, "$1" + y);

        let M = (utc ? date.getUTCMonth() : date.getMonth()) + 1;
        format = format.replace(/(^|[^\\])MMMM+/g, "$1" + MMMM[0]);
        format = format.replace(/(^|[^\\])MMM/g, "$1" + MMM[0]);
        format = format.replace(/(^|[^\\])MM/g, "$1" + this.ii(M));
        format = format.replace(/(^|[^\\])M/g, "$1" + M);

        let d = utc ? date.getUTCDate() : date.getDate();
        format = format.replace(/(^|[^\\])dddd+/g, "$1" + dddd[0]);
        format = format.replace(/(^|[^\\])ddd/g, "$1" + ddd[0]);
        format = format.replace(/(^|[^\\])dd/g, "$1" + this.ii(d));
        format = format.replace(/(^|[^\\])d/g, "$1" + d);

        let H = utc ? date.getUTCHours() : date.getHours();
        format = format.replace(/(^|[^\\])HH+/g, "$1" + this.ii(H));
        format = format.replace(/(^|[^\\])H/g, "$1" + H);

        let h = H > 12 ? H - 12 : H == 0 ? 12 : H;
        format = format.replace(/(^|[^\\])hh+/g, "$1" + this.ii(h));
        format = format.replace(/(^|[^\\])h/g, "$1" + h);

        let m = utc ? date.getUTCMinutes() : date.getMinutes();
        format = format.replace(/(^|[^\\])mm+/g, "$1" + this.ii(m));
        format = format.replace(/(^|[^\\])m/g, "$1" + m);

        let s = utc ? date.getUTCSeconds() : date.getSeconds();
        format = format.replace(/(^|[^\\])ss+/g, "$1" + this.ii(s));
        format = format.replace(/(^|[^\\])s/g, "$1" + s);

        let f = utc ? date.getUTCMilliseconds() : date.getMilliseconds();
        format = format.replace(/(^|[^\\])fff+/g, "$1" + this.ii(f, 3));
        f = Math.round(f / 10);
        format = format.replace(/(^|[^\\])ff/g, "$1" + this.ii(f));
        f = Math.round(f / 10);
        format = format.replace(/(^|[^\\])f/g, "$1" + f);

        let T = H < 12 ? "AM" : "PM";
        format = format.replace(/(^|[^\\])TT+/g, "$1" + T);
        format = format.replace(/(^|[^\\])T/g, "$1" + T.charAt(0));

        let t = T.toLowerCase();
        format = format.replace(/(^|[^\\])tt+/g, "$1" + t);
        format = format.replace(/(^|[^\\])t/g, "$1" + t.charAt(0));

        let tz = -date.getTimezoneOffset();
        let K = utc || !tz ? "Z" : tz > 0 ? "+" : "-";
        if (!utc) {
            tz = Math.abs(tz);
            let tzHrs = Math.floor(tz / 60);
            let tzMin = tz % 60;
            K += this.ii(tzHrs) + ":" + this.ii(tzMin);
        }
        format = format.replace(/(^|[^\\])K/g, "$1" + K);

        let day = (utc ? date.getUTCDay() : date.getDay()) + 1;
        format = format.replace(new RegExp(dddd[0], "g"), dddd[day]);
        format = format.replace(new RegExp(ddd[0], "g"), ddd[day]);

        format = format.replace(new RegExp(MMMM[0], "g"), MMMM[M]);
        format = format.replace(new RegExp(MMM[0], "g"), MMM[M]);

        format = format.replace(/\\(.)/g, "$1");

        return format;
    };

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

    public setAttributes(attributes: any[]): void {
        if (this.dataVisualisation) {
            this.dataVisualisation.setAttributes(attributes);
        }
    }
}
