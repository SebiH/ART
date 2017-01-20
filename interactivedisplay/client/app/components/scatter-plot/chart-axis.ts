import { ChartElement } from './chart-element';
import * as d3 from 'd3';

export enum AxisType {
    Vertical, Horizontal
}

export class ChartAxis {
    private svgElement: ChartElement;
    private scaleFn: d3.ScaleLinear<number, number>;

    constructor(
        root: ChartElement,
        private type: AxisType,
        private length: number,
        private offset?: number) {

        this.svgElement = root.append('g');
        this.scaleFn = d3.scaleLinear().range([0, length]).domain([0, 1]);
        this.paint();
    }

    public setDomain(min: number, max: number) {
        this.scaleFn.domain([min, max]);
        this.paint();
    }

    public scale(data: number): number {
        return this.scaleFn(data);
    }

    private paint() {
        if (this.type == AxisType.Horizontal) {
            this.svgElement
                .attr('transform', 'translate(0, ' + this.offset +')')
                .call(d3.axisBottom(this.scaleFn));
        } else {
            this.svgElement
                .call(d3.axisLeft(this.scaleFn));
        }
    }

    public remove() {
        if (this.svgElement) {
            this.svgElement.remove();
        }
    }

}
