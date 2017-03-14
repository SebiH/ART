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
            let x = scaleX(dataX[i]);
            let y = scaleY(dataY[i]);
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
        this.dataContainer.selectAll('circle').remove();
        this.hasData = false;
    }

    private processData(dim: ChartDimension): any[] {
        if (dim.isMetric) {
            return dim.data;
        } else {
            let domain = <string[]>_.map(dim.mappings, 'name');
            let converter = d => domain[d - dim.domain.min];
            return _.map(dim.data, converter);
        }
    }

    private initValues(data: number[][]): void {
        this.dataContainer.selectAll('.point')
            .data(data)
            .enter().append('circle')
                .attr('cx', d => d[0])
                .attr('cy', d => d[1])
                .attr('r', 5)
                .attr('class', 'point');
    }

    private animateValues(data: number[][]): void {
        this.dataContainer.selectAll('circle')
            .data(data)
            .transition()
            .duration(500)
            .ease(d3.easeLinear)
            .attr('cx', d => d[0])
            .attr('cy', d => d[1]);
    }
}
