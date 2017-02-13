import { ChartElement } from './chart-element';
import * as d3 from 'd3';

export class ChartData {

    private hasData: boolean = false;

    constructor(
        private chartRoot: ChartElement
        ) {
    }

    public clearData(): void {
        this.hasData = false;
        this.chartRoot.selectAll('circle').remove();
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
                .attr('cx', d => d[0])
                .attr('cy', d => d[1])
                .attr('r', 5)
                .attr('class', 'scatter-point');
    }

    private animateValues(data: number[][]): void {
        this.chartRoot.selectAll('circle')
            .data(data)
            .transition()
            .duration(500)
            .ease(d3.easeLinear)
            .attr('cx', d => d[0])
            .attr('cy', d => d[1]);
    }


    public highlight(selectedIds: number[], filteredIds: number[]): void {
        this.chartRoot.selectAll('circle')
            .each(function(d, i) {
                let isSelected = (selectedIds.indexOf(i) > -1);
                let isFiltered = (filteredIds && filteredIds.indexOf(i) < 0);
                let cssClass = 'scatter-point';

                let radius = 5;
                if (isFiltered && isSelected) {
                    radius = 5;
                } else if (isSelected) {
                    radius = 7;
                    cssClass += ' selected';
                } else if (isFiltered) {
                    radius = 3;
                    cssClass += ' filtered';
                }

                d3.select(this)
                    .attr('class', cssClass)
                    .attr('r', radius);
            });
    }
}
