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
    private yScale: d3.ScaleLinear<number, number> = null;

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


        // for later invert operations / selections
        let minVal: number = this.data[0].bin.range ? this.data[0].bin.range[0] : this.data[0].bin.value;
        let maxVal: number = this.data[0].bin.range ? this.data[0].bin.range[1] : this.data[0].bin.value;

        for (let d of this.data) {
            if (d.bin.range) {
                minVal = Math.min(minVal, d.bin.range[0]);
                maxVal = Math.max(maxVal, d.bin.range[1]);
            } else {
                minVal = Math.min(minVal, d.bin.value);
                maxVal = Math.max(maxVal, d.bin.value);
            }
        }

        this.yScale = d3.scaleLinear()
            .range([0, height])
            .domain([minVal, maxVal]);

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
    }

    public unregister(): void {
        this.dataContainer.remove();
    }


    public resize(width: number, height: number): void {
        // TODO.
    }

    // graph pos -> actual data val
    public invert(val: number): number {
        return this.yScale.invert(val);
    }

    public setRanges(ranges: [number, number][]) {
        this.rangeContainer.html('');
        let baseUrl = Utils.getBaseUrl();

        for (let range of ranges) {
            let start = this.yScale(range[0]);
            let end = this.yScale(range[1]);

            this.rangeContainer.append('rect')
                .attr('width', this.width)
                .attr('height', end - start)
                .attr('y', start)
                .attr('transform', 'translate(-2,0)') // -2 due to borders
                .style('fill', '#f44336')
                .attr('opacity', '0.5');
        }

    }
}
