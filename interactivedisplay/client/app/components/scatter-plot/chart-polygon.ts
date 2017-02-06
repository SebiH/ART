import { ChartElement } from './chart-element';
import { Point } from '../../models/index';
import * as d3 from 'd3';

// documentation: https://riccardoscalco.github.io/textures/
declare var textures: any;

export class ChartPolygon {
    private pathElement: ChartElement;
    private line: d3.Line<number[]>;

    private selectedTextureFill: string;
    private normalTextureFill: string;

    public constructor(svg: ChartElement) {
        let normalTexture = textures.lines().thicker();
        svg.call(normalTexture);
        this.normalTextureFill = this.getAbsoluteTextureUrl(normalTexture.url());

        let selectedTexture = textures.lines().heavier(5).thinner(1.5).stroke('#F44336');
        svg.call(selectedTexture);
        this.selectedTextureFill = this.getAbsoluteTextureUrl(selectedTexture.url());

        this.pathElement = svg.append('path')
            .attr('class', 'line')
            .attr('fill', this.normalTextureFill);

        this.line = d3.line()
            .curve(d3.curveBasisClosed)
            .x(d => d[0])
            .y(d => d[1]);
    }

    // Texture.js returns relative url as 'url(#xyz)', 
    // but Edge/Firefox need absolute url: 'url(localhost#324)'
    private getAbsoluteTextureUrl(url: string) {
        let baseUrl = window.location.href;
        return url.replace('url(', 'url(' + baseUrl);
    }

    public paint(path: number[][]) {
        this.pathElement.attr('d', this.line(path));
    }

    public setSelected(isSelected: boolean) {
        if (isSelected) {
            this.pathElement
                .attr('class', 'line selected')
                .attr('fill', this.selectedTextureFill);
        } else {
            this.pathElement
                .attr('class', 'line')
                .attr('fill', this.normalTextureFill);
        }
    }

    public remove() {
        this.pathElement.remove();
    }
}
