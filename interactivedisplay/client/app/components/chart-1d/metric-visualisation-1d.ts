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
    private yBarScale: d3.ScaleBand<string> = null;

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
        **    Bars
        **/

        let x = d3.scalePow()
            .exponent(0.5)
            .rangeRound([0, width])
            .domain([0, _.maxBy(this.data, 'amount').amount * 1.1]);

        let binNames = <string[]>_.map(this.data, 'bin.displayName');
        let y = d3.scaleBand()
            .range([0, height])
            .domain(binNames);
        this.yBarScale = y;

        let range = this.getRange();
        this.yScale = d3.scaleLinear()
            .range([0, height])
            .domain([range.min, range.max]);

        this.dataContainer.selectAll('.bar')
            .data(this.data)
            .enter().append('rect')
                .attr('class', 'bar')
                .attr('fill', (d, i) => Utils.getGradientColor(this.dimension.gradient, y(d.bin.displayName) / height))
                .attr('x', 0)
                .attr('y', d => y(d.bin.displayName))
                .attr('height', y.bandwidth())
                .attr('width', d => x(d.amount));


        /*
        **    Labels
        **/

        for (let i = 0; i < this.data.length; i++) {

            let lineHeight = y(this.data[i].bin.displayName) + y.bandwidth() + TEXT_Y_OFFSET;

            // label
            this.dataContainer.append('text')
                .text(this.data[i].bin.displayName)
                .attr('class', 'tick-label line-tick-label')
                .attr('x', TEXT_X_OFFSET)
                .attr('y', lineHeight);
        }


        this.rangeContainer = this.dataContainer.append('g');
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

        let minHeight = this.height;
        let maxHeight = 0;

        if (ranges.length > 0) {
            // build the inverse of all ranges, black out everything *but* the actual ranges
            let domain = this.getRange();

            let invRanges: [number, number][] = [[domain.min, domain.max]];

            for (let range of ranges) {
                for (let i = invRanges.length - 1; i >= 0; i--) {
                    let invRange = invRanges[i];

                    let isMinInRange = (invRange[0] <= range[0] && range[0] <= invRange[1]);
                    let isMaxInRange = (invRange[0] <= range[1] && range[1] <= invRange[1]);

                    if (isMinInRange && isMaxInRange) {
                        // split into two
                        _.pullAt(invRanges, i);
                        if (invRange[0] != range[0]) {
                            invRanges.push([invRange[0], range[0]]);
                        }
                        if (range[1] != invRange[1]) {
                            invRanges.push([range[1], invRange[1]]);
                        }
                    } else if (isMinInRange) {
                        invRange[1] = range[0];
                    } else if (isMaxInRange) {
                        invRange[0] = range[1];
                    }

                    let isRangeInverted = invRange[0] >= invRange[1];
                    let isInvRangeWithinRange = range[0] <= invRange[0] && invRange[1] <= range[1];
                    if (isRangeInverted || isInvRangeWithinRange) {
                        _.pullAt(invRanges, i);
                    }
                }
            }

            for (let range of invRanges) {
                let start = this.yScale(range[0]);
                let end = this.yScale(range[1]);

                minHeight = Math.min(end, minHeight);
                maxHeight = Math.max(start, maxHeight);

                this.rangeContainer.append('rect')
                    .attr('width', this.width)
                    .attr('height', end - start)
                    .attr('y', start)
                    .attr('transform', 'translate(-2,0)') // -2 due to borders
                    .style('fill', '#000000')
                    .attr('opacity', '0.75');
            }
        }

        if (maxHeight <= 0) {
            maxHeight = this.height;
        }

        if (minHeight >= this.height) {
            minHeight = 0;
        }

        if (minHeight > maxHeight) {
            let temp = minHeight;
            minHeight = maxHeight;
            maxHeight = temp;
        }

        this.dataContainer.selectAll('.bar')
            .attr('fill', (d: any, i) => {
                let offset = 0;

                if (d.bin.range !== undefined) {
                    offset = (this.yScale(d.bin.range[0]) - minHeight) / (maxHeight - minHeight);
                    if (offset < 0 || offset > 1) {
                        offset = (this.yScale(d.bin.range[1]) - minHeight) / (maxHeight - minHeight);
                    }
                } else {
                    let barWidth = this.yBarScale.bandwidth();
                    offset = (this.yScale(d.bin.value) - minHeight - barWidth / 2) / (maxHeight - minHeight);
                    if (offset < 0 || offset > 1) {
                        offset = (this.yScale(d.bin.value) - minHeight + barWidth / 2) / (maxHeight - minHeight);
                    }
                }

                if (offset < 0 || offset > 1) {
                    return '#9E9E9E';
                } else {
                    return Utils.getGradientColor(this.dimension.gradient, offset);
                }
            });
    }


    private getRange() {
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

        return { min: minVal, max: maxVal };
    }
}
