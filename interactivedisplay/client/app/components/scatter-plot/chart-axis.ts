import { ChartElement } from './chart-element';
import * as d3 from 'd3';

export class ChartAxis {
    private svgElement: ChartElement;
    public scale;

    constructor(
        root: ChartElement,
        private type: 'x' | 'y',
        private length: number,
        private offset: number) {

        this.svgElement = root.append('g');
        this.scale = d3.scaleLinear().range([0, length]).domain([0, 1]);
        this.paint();
    }

    public setDomainLinear(min: number, max: number) {
        let range = this.type === 'x' ? [0, this.length] : [this.length, 0];

        this.scale = d3.scaleLinear()
            .range(range)
            .domain([min, max]);
        this.paint();
    }

    public setDomainCategorical(domain: string[]) {
        // create gap between axis start/end
        domain.unshift('');
        domain.push(' ');

        if (this.type === 'y') {
            let flipped: string[] = [];
            for (let i = 0; i < domain.length; i++) {
                flipped[domain.length - 1 - i] = domain[i];
            }
            domain = flipped;
        }

        this.scale = d3.scalePoint()
            .domain(domain)
            .range([0, this.length]);
        this.paint();
    }

    private paint() {
        if (this.type === 'x') {
            this.svgElement
                .attr('transform', 'translate(0, ' + this.offset +')')
                .call(d3.axisBottom(this.scale));
        } else {
            this.svgElement
                .attr('transform', 'translate(' + (-this.offset) + ', 0)')
                .call(d3.axisLeft(this.scale));
        }
    }

    public remove() {
        if (this.svgElement) {
            this.svgElement.remove();
        }
    }

}
