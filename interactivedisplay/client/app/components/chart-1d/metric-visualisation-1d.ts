import { HtmlChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';
import { ChartVisualisation1d } from './chart-visualisation-1d';
import { Utils } from '../../Utils';

import * as d3 from 'd3';
import * as _ from 'lodash';

const TEXT_X_OFFSET = 5;
const TEXT_Y_OFFSET = -10;

interface Bin { displayName: string, value?: number, range?: [number, number] }

export class MetricVisualisation1d extends ChartVisualisation1d {

    private dataContainer: HtmlChartElement;
    private rangeContainer: HtmlChartElement;
    private yScale: d3.ScaleBand<string> = null;

    private data: { bin: Bin, amount: number }[] = [];

    private width: number;
    private height: number;

    public constructor(public dimension: ChartDimension) {
        super();

        for (let bin of dimension.bins) {
            this.data.push({ bin: bin, amount: 0 });
        }

        for (let data of dimension.data) {
            for (let container of this.data) {
                let bin = container.bin;
                if (bin.value !== undefined) {
                    if (bin.value === Math.floor(data)) {
                        container.amount += 1;
                        break;
                    }
                } else {
                    if (bin.range[0] <= data && bin.range[1] >= data) {
                        container.amount += 1;
                        break;
                    }
                }
            }
        }


        // remove empty containers at the edges
        while (this.data[0].amount == 0) {
            this.data.shift();
        }

        while (this.data[this.data.length - 1].amount == 0) {
            this.data.pop();
        }
    }

    public register(root: HtmlChartElement, width: number, height: number): void {

        this.width = width;
        this.height = height;

        this.dataContainer = root.append('g');

        /*
        **    Gradient
        **/
        // let id = Utils.getId();

        // let gradient = this.dataContainer.append('defs').append('linearGradient')
        //     .attr('id', 'gradient' + id)
        //     .attr('x1', '100%')
        //     .attr('x2', '100%')
        //     .attr('y1', '100%')
        //     .attr('y2', '0%');

        // for (let gradientStop of this.dimension.gradient) {
        //     gradient.append('stop')
        //         .attr('offset', (gradientStop.stop * 100) + '%')
        //         .attr('stop-color', gradientStop.color)
        //         .attr('stop-opacity', 0.8);
        // }


        // let baseUrl = Utils.getBaseUrl();
        // let background = this.dataContainer.append('rect')
        //     .attr('width', width - 2) // - 2 due to borders..
        //     .attr('height', height)
        //     .attr('transform', 'translate(-2,0)') // -2 due to borders
        //     .style('fill', 'url(' + baseUrl + '#gradient' + id + ')');


        /*
        **    Bars
        **/

        let x = d3.scaleLinear()
            .rangeRound([0, width])
            .domain([0, _.maxBy(this.data, 'amount').amount * 1.1]);

        let binNames = <string[]>_.map(this.data, 'bin.displayName');
        let y = d3.scaleBand()
            .range([0, height])
            .domain(binNames);

        this.yScale = y;

        this.dataContainer.selectAll('.bar')
            .data(this.data)
            .enter().append('rect')
                .attr('class', 'bar')
                .attr('fill', (d, i) => Utils.getGradientColor(this.dimension.gradient, y(d.bin.displayName) / height))
                .attr('x', 0)
                .attr('y', d => y(d.bin.displayName))
                .attr('height', y.bandwidth())
                .attr('width', d => x(d.amount));

        this.rangeContainer = this.dataContainer.append('g');

        /*
        **    Labels
        **/

        // for (let i = 0; i < this.dimension.bins.length; i++) {

        //     let lineHeight = y(i + 1);

        //     // line
        //     let tickLine = d3.line();
        //     let tickLinePath = this.dataContainer.append('path')
        //         .datum([[0, lineHeight], [width, lineHeight]])
        //         .attr('class', 'tick-line')
        //         .attr('d', tickLine);

        //     // label
        //     this.dataContainer.append('text')
        //         .text(this.dimension.bins[i].displayName)
        //         .attr('class', 'tick-label line-tick-label')
        //         .attr('x', TEXT_X_OFFSET)
        //         .attr('y', lineHeight + TEXT_Y_OFFSET);
        // }



        // highlight actual data points
        // let dots = this.dataContainer.selectAll('.line-point')
        //     .data(this.bins)
        //     .enter().append('circle')
        //         .attr('cx', (d) => x(d))
        //         .attr('cy', (d, i) => y(i + 1))
        //         .attr('r', 10)
        //         .attr('class', 'line-point');

    }

    public unregister(): void {
        this.dataContainer.remove();
    }


    public resize(width: number, height: number): void {
        // TODO.
    }

    public invert(val: number): number {
        return -1;
        // return this.yScale.invert(val)
    }

    // graph pos -> actual data val
    // TODO: this is a whole lot more complicated than it should be
    public convertData(val: number): number {
        // let scaleOffset = this.yScale.invert(val)
        // let binIndex = Math.floor(scaleOffset - 1);
        // let remainder = scaleOffset % 1;

        // if (binIndex < 0) {
        //     remainder = 0;
        //     binIndex = 0;
        // } else if (binIndex >= this.bins.length) {
        //     binIndex = this.bins.length - 1;
        //     remainder = 1;
        // }

        // if (this.dimension.bins[binIndex] != null) {
        //     let bin = this.dimension.bins[binIndex];

        //     if (bin.value != undefined) {
        //         let nextBin = this.dimension.bins[binIndex + 1];
        //         if (nextBin) {
        //             let dist = this.minVal(nextBin) - bin.value;
        //             return bin.value + dist * remainder;
        //         } else {
        //             return bin.value;
        //         }
        //     } else {
        //         let range = bin.range[1] - bin.range[0];
        //         return bin.range[0] + remainder * range;
        //     }
        // }

        return -1;
    }

    // actual data value -> graph position
    public invertData(val: number): number {
        let bins = this.dimension.bins;

        for (let i = 0; i < bins.length; i++) {
            let bin = bins[i];

            if (bin.value != undefined) {

                let nextBin = bins[i + 1];

                if (bin.value == val) {
                    return i + 1;
                } else if (nextBin && this.isBetween(val, bin, nextBin)) {
                    let dist = this.minVal(nextBin) - bin.value;
                    let remainder = val % 1;

                    // ... magic!
                    if (dist < 0) {
                        return i + 2 + remainder * dist;
                    } else {
                        return i + 1 + remainder * dist;
                    }
                }

            } else {
                if (bin.range[0] <= val && bin.range[1] >= val) {
                    let percent = (val - bin.range[0]) / (bin.range[1] - bin.range[0]);
                    return i + 1 + percent;
                }
            }
        }

        return -1;
    }

    private minVal(bin: any): number {
        return bin.value === undefined ? bin.range[0] : bin.value;
    }

    private isBetween(val: number, bin1: any, bin2: any): boolean {
        if (bin1.value < this.minVal(bin2)) {
            return bin1.value <= val && val <= this.minVal(bin2);
        } else {
            return this.minVal(bin2) <= val && val <= bin1.value;
        }
    }

    public setRanges(ranges: [number, number][]) {
        // this.rangeContainer.html('');
        // let baseUrl = Utils.getBaseUrl();

        // for (let range of ranges) {
        //     let start = this.yScale(range[1]);
        //     let end = this.yScale(range[0]);

        //     this.rangeContainer.append('rect')
        //         .attr('width', this.width)
        //         .attr('height', end - start)
        //         .attr('y', start)
        //         .attr('transform', 'translate(-2,0)') // -2 due to borders
        //         .style('fill', '#f44336')
        //         .attr('opacity', '0.5');
        // }

    }
}
