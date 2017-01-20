import { ChartElement } from './chart-element';
import { Point } from '../../models/index';
import * as d3 from 'd3';

// documentation: https://riccardoscalco.github.io/textures/
declare var textures: any;

export class ChartPolygon {
    private pathElement: ChartElement;
    private line: d3.Line<Point>;

    public init(svg: ChartElement) {
        let texture = textures.lines().thicker();
        svg.call(texture);

        this.pathElement = svg.append('path')
            .attr('stroke', 'blue')
            .attr('stroke-width', 2)
            .attr('fill-opacity', '0.4')
            .attr('fill', texture.url());

        this.line = d3.line<Point>()
            .curve(d3.curveBasisClosed)
            .x(d => d.x)
            .y(d => d.y);
    }

    public paint(path: Point[]) {
        this.pathElement.attr('d', this.line(path));
    }

    public remove() {
        this.pathElement.remove();
    }
}
