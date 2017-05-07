import { ChartDimension } from '../../models/index';
import { HtmlChartElement, ChartElement } from '../../directives/index';

import * as _ from 'lodash';
import * as d3 from 'd3';

export class ScatterplotVisualisation extends ChartElement {

    private dataContainer: HtmlChartElement;
    private hasData: boolean = false;

    public register(root: HtmlChartElement, width: number, height: number): void {
        this.dataContainer = root.append('g');
    }

    public unregister(): void {
        this.dataContainer.remove();
    }

    public resize(width: number, height: number): void {

    }

    public loadData(dimX: ChartDimension, dimY: ChartDimension, scaleX: any, scaleY: any): void {
        let dataX = this.processData(dimX);
        let dataY = this.processData(dimY);
        let data: number[][] = [];

        // assuming dimX data length === dimY data length
        for (let i = 0; i < dimX.data.length; i++) {
            let x = scaleX(dataX[i].value);
            let y = scaleY(dataY[i].value);
            data.push([x, y]);
        }

        if (!this.hasData) {
            this.hasData = true;
            this.initValues(data);
        } else {
            this.animateValues(data);
        }
    }


    public clearData(): void {
        this.dataContainer.selectAll('rect').remove();
        this.hasData = false;
    }

    private processData(dim: ChartDimension): any[] {
        // if (dim.isMetric) {
            return dim.data;
        // } else {
        //     let domain = <string[]>_.map(dim.mappings, 'name');
        //     let converter = d => domain[d - dim.domain.min];
        //     return _.map(dim.data, converter);
        // }
    }

    private initValues(data: number[][]): void {
        this.dataContainer.selectAll('.point')
            .data(data)
            .enter().append('rect')
                .attr('x', d => d[0] - 5)
                .attr('y', d => d[1] - 5)
                .attr('width', 10)
                .attr('height', 10)
                .attr('class', 'point')
                .attr('fill', '#000000');
    }

    private animateValues(data: number[][]): void {
        this.dataContainer.selectAll('rect')
            .data(data)
            .transition()
            .duration(500)
            .ease(d3.easeLinear)
            .attr('x', d => d[0] - 5)
            .attr('y', d => d[1] - 5);
    }

    public setAttributes(attributes: any[]): void {
        this.dataContainer.selectAll('rect')
            .attr('fill', (d, i) => attributes[i].fill)
            .attr('stroke', (d, i) => attributes[i].stroke)
            .attr('width', (d, i) => attributes[i].radius)
            .attr('height', (d, i) => attributes[i].radius);
    }
}
