import { ChartElement } from './chart-element';
import { Point } from '../../models/index';
import * as d3 from 'd3';

// documentation: https://riccardoscalco.github.io/textures/
declare var textures: any;

export class ChartPolygon {
    private pathElement: ChartElement;
    private line: d3.Line<Point>;

    private selectedTexture;
    private normalTexture;

    public constructor(svg: ChartElement) {
        this.normalTexture = textures.lines().thicker();
        svg.call(this.normalTexture);

        this.selectedTexture = textures.lines().heavier(5).thinner(1.5).stroke('#F44336');
        svg.call(this.selectedTexture);

        this.pathElement = svg.append('path')
            .attr('class', 'line')
            .attr('fill', this.normalTexture.url());

        this.line = d3.line<Point>()
            .curve(d3.curveBasisClosed)
            .x(d => d.x)
            .y(d => d.y);
    }

    public paint(path: Point[]) {
        this.pathElement.attr('d', this.line(path));
    }

    public setSelected(isSelected: boolean) {
        if (isSelected) {
            this.pathElement
                .attr('class', 'line selected')
                .attr('fill', this.selectedTexture.url());
        } else {
            this.pathElement
                .attr('class', 'line')
                .attr('fill', this.normalTexture.url());
        }
    }

    public remove() {
        this.pathElement.remove();
    }
}
