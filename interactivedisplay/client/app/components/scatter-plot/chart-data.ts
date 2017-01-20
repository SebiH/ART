import { ChartElement } from './chart-element';
import * as d3 from 'd3';

export class ChartData {

    private hasData: boolean = false;

    constructor(
        private chartRoot: ChartElement,
        private scaleX: d3.ScaleLinear<number, number>,
        private scaleY: d3.ScaleLinear<number, number>
        ) {
    }

    public clearData(): void {
        // TODO
        console.log('nyi');
    }

    public setData(data: number[][]): void {
        if (!this.hasData) {
            this.hasData = true;
            this.initValues(data);
        } else {
            this.animateValues(data);
        }
    }

    private initValues(data: number[][]): void {
        this.chartRoot.selectAll('dot')
            .data(data)
            .enter().append('circle')
                .attr('r', 5)
                .attr('cx', d => this.scaleX(d[0]))
                .attr('cy', d => this.scaleY(d[1]));
    }

    private animateValues(data: number[][]): void {
        this.chartRoot.selectAll('circle')
            .data(data)
            .transition()
            .duration(200)
            .ease(d3.easeLinear)
            .attr('cx', d => this.scaleX(d[0]))
            .attr('cy', d => this.scaleY(d[1]));
    }
}
