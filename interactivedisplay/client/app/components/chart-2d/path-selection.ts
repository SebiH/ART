import { Subscription } from 'Rxjs/Subscription';
import { Chart2dComponent } from './chart-2d.component';
import { HtmlChartElement, ChartElement } from '../../directives/index';
import { Filter  } from '../../models/index';
import { Utils } from '../../Utils';
import * as d3 from 'd3';

// documentation: https://riccardoscalco.github.io/textures/
declare var textures: any;

export class PathSelection extends ChartElement {

    private pathElement: HtmlChartElement;
    private textureFill: string;
    private line: d3.Line<number[]>;
    private filterSub: Subscription;

    public constructor(public id: number, private parent: Chart2dComponent, private filter: Filter) {
        super();
    }

    public register(root: HtmlChartElement, width: number, height: number): void {
        // let texture = textures.lines().thicker().stroke(this.color);
        // root.call(texture);
        // this.textureFill = this.getAbsoluteTextureUrl(texture.url());

        this.pathElement = root.append('path')
            .attr('class', 'line')
            .attr('stroke', this.filter.getColor())
            .attr('fill', this.filter.getColor())
            // .attr('fill', this.textureFill);

        this.line = d3.line()
            .curve(d3.curveLinearClosed)
            .x(d => this.parent.xScale(d[0]))
            .y(d => this.parent.yScale(d[1]));


        this.filterSub =  this.filter.onUpdate
            .subscribe(() => {
                this.setColor();
            })
    }

    public unregister(): void {
        // TODO: remove texture?
        this.pathElement.remove();
        this.filterSub.unsubscribe();
    }

    public resize(width: number, height: number): void {
        // TODO!
    }

    public setPath(path: number[][]): void {
        this.pathElement.attr('d', this.line(path));
    }

    private setColor(): void {
        if (this.filter.isSelected) {
            this.pathElement
                .attr('stroke', 'black')
                .attr('fill', 'black');
        } else {
            this.pathElement
                .attr('stroke', this.filter.getColor())
                .attr('fill', this.filter.getColor());
        }
    }

    // Texture.js returns relative url as 'url(#xyz)',
    // but Edge/Firefox need absolute url: 'url(localhost#324)'
    private getAbsoluteTextureUrl(url: string) {
        let baseUrl = Utils.getBaseUrl();
        return url.replace('url(', 'url(' + baseUrl);
    }
}
