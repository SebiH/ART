import { HtmlChartElement, ChartElement } from '../../directives/index';
import { Utils } from '../../Utils';
import * as d3 from 'd3';

// documentation: https://riccardoscalco.github.io/textures/
declare var textures: any;

export class PathSelection extends ChartElement {

    private pathElement: HtmlChartElement;
    private textureFill: string;
    private line: d3.Line<number[]>;

    public constructor(private color: string) {
        super();
    }

    public register(root: HtmlChartElement, width: number, height: number): void {
        let texture = textures.lines().thicker().stroke(this.color);
        root.call(texture);
        this.textureFill = this.getAbsoluteTextureUrl(texture.url());

        this.pathElement = root.append('path')
            .attr('class', 'line')
            .attr('stroke', this.color)
            .attr('fill', this.textureFill);

        this.line = d3.line()
            .curve(d3.curveBasisClosed)
            .x(d => d[0])
            .y(d => d[1]);
    }

    public unregister(): void {
        // TODO: remove texture?
        this.pathElement.remove();
    }

    public resize(width: number, height: number): void {
        // TODO!
    }

    public setPath(path: number[][]): void {
        this.pathElement.attr('d', this.line(path));
    }


    // Texture.js returns relative url as 'url(#xyz)', 
    // but Edge/Firefox need absolute url: 'url(localhost#324)'
    private getAbsoluteTextureUrl(url: string) {
        let baseUrl = Utils.getBaseUrl();
        return url.replace('url(', 'url(' + baseUrl);
    }
}
