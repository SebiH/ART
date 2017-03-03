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

export class LineVisualisation1d extends ChartVisualisation1d {

    private dataContainer: HtmlChartElement;

    public constructor(public dimension: ChartDimension) {
        super();
    }

    public register(root: HtmlChartElement, width: number, height: number): void {
        this.dataContainer = root.append('g');
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
            .attr('width', width)
            .attr('height', height)
            .style('fill', 'url(' + baseUrl + '#gradient)');
    }


    public unregister(): void {
        this.dataContainer.remove();
    }


    public resize(width: number, height: number): void {
        // TODO.
    }

}
