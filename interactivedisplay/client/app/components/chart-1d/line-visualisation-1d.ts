import { HtmlChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';
import { ChartVisualisation1d } from './chart-visualisation-1d';
import { Utils } from '../../Utils';

import * as d3 from 'd3';
import * as _ from 'lodash';

const GRADIENT_COLOURS = [
    '#E3F2FD', // start: blue 50
    '#0D47A1', // end: blue 900
];

const NUM_BINS = 10;

export class LineVisualisation1d extends ChartVisualisation1d {

    private dataContainer: HtmlChartElement;
    private bins: number[] = [];
    private yScale: d3.ScaleLinear<number, number> = null;

    public constructor(public dimension: ChartDimension) {
        super();

        for (let i = 0; i < NUM_BINS; i++) {
            this.bins[i] = 0;
        }

        for (let data of dimension.data) {
            let val = (data / dimension.domain.max) * NUM_BINS;
            let binIndex = Math.floor(val);
            this.bins[binIndex] += 1;
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

        gradient.append('stop')
            .attr('offset', '0%')
            .attr('stop-color', GRADIENT_COLOURS[0])
            .attr('stop-opacity', 0.8);

        gradient.append('stop')
            .attr('offset', '100%')
            .attr('stop-color', GRADIENT_COLOURS[1])
            .attr('stop-opacity', 0.8);

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
            .domain([0, NUM_BINS + 1])
            .range([0, height]);
        this.yScale = y;

        let line = d3.area<number>()
            .x0(() => width)
            .x1((d, i) => x(d))
            .y((d, i) => y(i));

        let paddedBins = [];
        paddedBins[0] = this.bins[0];
        for (let i = 1; i < NUM_BINS + 1; i++) {
            paddedBins[i] = this.bins[i - 1];
        }
        paddedBins[NUM_BINS + 1] = this.bins[NUM_BINS - 1];

        let linePath = this.dataContainer.append('path')
            .datum(paddedBins)
            .attr('fill', 'white')
            .attr('stroke', 'black')
            .attr('stroke-width', '2px')
            .attr('d', line);

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
