import { HtmlChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';
import { ChartVisualisation1d } from './chart-visualisation-1d';
import { Utils } from '../../Utils';

import * as d3 from 'd3';
import * as _ from 'lodash';

const TEXT_X_OFFSET = 5;
const TEXT_Y_OFFSET = -10;

export class LineVisualisation1d extends ChartVisualisation1d {

    private dataContainer: HtmlChartElement;
    private bins: number[] = [];
    private yScale: d3.ScaleLinear<number, number> = null;

    public constructor(public dimension: ChartDimension) {
        super();

        for (let i = 0; i < dimension.bins.length; i++) {
            this.bins[i] = 0;
        }

        for (let data of dimension.data) {
            let binIndex = 0;
            for (let bin of dimension.bins) {
                if (bin.value !== undefined) {
                    if (bin.value === Math.floor(data)) {
                        this.bins[binIndex] += 1;
                        break;
                    }
                } else {
                    if (bin.range[0] <= data && bin.range[1] >= data) {
                        this.bins[binIndex] += 1;
                        break;
                    }
                }
                binIndex++;
            }
        }
    }

    public register(root: HtmlChartElement, width: number, height: number): void {

        this.dataContainer = root.append('g');

        /*
        **    Gradient
        **/
        let gradient = this.dataContainer.append('defs').append('linearGradient')
            .attr('id', 'gradient')
            .attr('x1', '100%')
            .attr('x2', '100%')
            .attr('y1', '0%')
            .attr('y2', '100%');

        for (let gradientStop of this.dimension.gradient) {
            let percent = Math.floor((gradientStop.stop / this.dimension.domain.max) * 100);
            gradient.append('stop')
                .attr('offset', percent + '%')
                .attr('stop-color', gradientStop.color)
                .attr('stop-opacity', 0.8);
        }

        let baseUrl = Utils.getBaseUrl();
        let background = this.dataContainer.append('rect')
            .attr('width', width - 2) // - 2 due to borders..
            .attr('height', height)
            .attr('transform', 'translate(-2,0)') // -2 due to borders
            .style('fill', 'url(' + baseUrl + '#gradient)');


        /*
        **    Line
        **/

        let domain = [0, _.max(this.bins) * 1.2];
        let x = d3.scaleLinear()
            .domain(domain)
            .range([width, 0]);

        let y = d3.scaleLinear()
            .domain([0, this.bins.length + 1])
            .range([0, height]);
        this.yScale = y;

        let line = d3.area<number>()
            .x0(() => width)
            .x1((d, i) => x(d))
            .y((d, i) => y(i));

        let paddedBins = [];
        paddedBins[0] = this.bins[0];
        for (let i = 1; i < this.bins.length + 1; i++) {
            paddedBins[i] = this.bins[i - 1];
        }
        paddedBins[this.bins.length + 1] = this.bins[this.bins.length - 1];

        let linePath = this.dataContainer.append('path')
            .datum(paddedBins)
            .attr('fill', 'white')
            .attr('stroke', 'black')
            .attr('stroke-width', '2px')
            .attr('d', line);


        /*
        **    Labels
        **/

        for (let i = 0; i < this.dimension.bins.length; i++) {

            let lineHeight = y(i + 1);

            // line
            let tickLine = d3.line();
            let tickLinePath = this.dataContainer.append('path')
                .datum([[0, lineHeight], [width, lineHeight]])
                .attr('class', 'tick-line')
                .attr('d', tickLine);

            // label
            this.dataContainer.append('text')
                .text(this.dimension.bins[i].displayName)
                .attr('class', 'tick-label line-tick-label')
                .attr('x', TEXT_X_OFFSET)
                .attr('y', lineHeight + TEXT_Y_OFFSET);
        }



        // highlight actual data points
        let dots = this.dataContainer.selectAll('.line-point')
            .data(this.bins)
            .enter().append('circle')
                .attr('cx', (d) => x(d))
                .attr('cy', (d, i) => y(i + 1))
                .attr('r', 10)
                .attr('class', 'line-point');

    }


    public unregister(): void {
        this.dataContainer.remove();
    }


    public resize(width: number, height: number): void {
        // TODO.
    }

    public invert(val: number): number {
        return this.yScale.invert(val);
    }
}
