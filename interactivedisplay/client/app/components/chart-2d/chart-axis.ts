import { ChartDimension } from '../../models/index';
import { HtmlChartElement, ChartElement } from '../../directives/index';
import * as d3 from 'd3';

export class ChartAxis extends ChartElement {

    private svgElement: HtmlChartElement;
    private width: number;
    private height: number;

    public constructor(private type: 'x' | 'y') {
        super();
    }

    public register(root: HtmlChartElement, width: number, height: number): void {
        this.svgElement = root.append('g');
        this.width = width;
        this.height = height;
    }

    public unregister(): void {
        this.svgElement.remove();
    }

    public resize(width: number, height: number): void {
        // TODO
    }

    public setScale(scale: any): void {
        if (this.type === 'x') {
            this.svgElement
                .attr('transform', 'translate(0, ' + this.height +')')
                .call(d3.axisBottom(scale));
        } else {
            this.svgElement
                .call(d3.axisLeft(scale));
        }
    }
}
