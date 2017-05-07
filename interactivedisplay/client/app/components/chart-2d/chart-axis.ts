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

        this.svgElement
            .attr('class', 'axis')
            .selectAll('text')
                .attr('font-size', '26px')
                .call(this.wrap, 160, this.type);
    }

    // see: https://bl.ocks.org/mbostock/7555321
    private wrap(text, width, axis) {
        text.each(function() {
            let text = d3.select(this);
            let words = text.text().trim().split(/\s+/).reverse();
            let word;
            let line = [];
            let lineNumber = 0;
            let lineHeight = 1.1; // ems
            let y = text.attr("y");
            let dy = parseFloat(text.attr("dy")) + (axis == 'x' ? 0.5 : 0);
            let dx = axis == 'x' ? 0 : -1;
            let tspan = text.text(null).append("tspan")
                .attr("x", dx + "em")
                .attr("y", y)
                .attr("dx", 0)
                .attr("dy", dy + "em");

            while (word = words.pop()) {
                line.push(word);
                tspan.text(line.join(" "));
                if ((<any>tspan.node()).getComputedTextLength() > width) {
                    line.pop();
                    tspan.text(line.join(" "));
                    line = [word];
                    tspan = text.append("tspan")
                        .attr("x", dx + "em")
                        .attr("y", y)
                        .attr("dy", ++lineNumber * lineHeight + dy + "em")
                        .attr("dx", 0)
                        .text(word);
                }
            }
        });
    }

}
