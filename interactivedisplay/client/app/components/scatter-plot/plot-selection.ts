import { PlotElement } from './plot-element';
import { Point } from '../../models/index';
import * as d3 from 'd3';

// documentation: https://riccardoscalco.github.io/textures/
declare var textures: any;

export class PlotSelection {
    private pathElement: PlotElement;

    public init(svg: PlotElement) {
        let texture = textures.lines().thicker();
        svg.call(texture);

        this.pathElement = svg.append('path')
            .attr('stroke', 'blue')
            .attr('stroke-width', 2)
            .attr('fill-opacity', '0.4')
            .attr('fill', texture.url());
    }

    public paint(path: Point[]) {
        let polygonLine = d3.line<Point>()
            .curve(d3.curveBasisClosed)
            .x(d => d.x)
            .y(d => d.y);
        this.pathElement.attr('d', polygonLine(path));
    }

    public remove() {
        this.pathElement.remove();
    }
}
