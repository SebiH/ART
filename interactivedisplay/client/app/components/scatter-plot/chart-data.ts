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
                .attr('fill', 'black');
    }

    private animateValues(data: number[][]): void {
        this.chartRoot.selectAll('circle')
            .data(data)
            .transition()
            .duration(200)
            .ease(d3.easeLinear)
            .attr('cx', d => d[0])
            .attr('cy', d => d[1]);
    }


    public highlight(selectedIds: number[], filteredIds: number[], useFilter: boolean): void {
        this.chartRoot.selectAll('circle')
            // .attr('class', (d, i) => {
            //     let cssClass = 'data';
            //     if (selectedIds.indexOf(i) > -1) {
            //         cssClass += ' selected';
            //     }
            //     if (useFilter && filteredIds.indexOf(i) < 0) {
            //         cssClass += ' filtered';
            //     }

            //     return cssClass;
            // });
            // .transition()
            // .duration(100)
            // .ease(d3.easeLinear)
            .each(function(d, i) {
                let isSelected = (selectedIds.indexOf(i) > -1);
                let isFiltered = (useFilter && filteredIds.indexOf(i) < 0);

                let radius = 5;
                let fill = 'black';
                if (isFiltered && isSelected) {
                    fill = 'red';
                    radius = 5;
                } else if (isSelected) {
                    radius = 7;
                    fill = 'green';
                } else if (isFiltered) {
                    radius = 3;
                    fill = 'gray';
                }

                d3.select(this)
                    .transition()
                    .duration(200)
                    .ease(d3.easeLinear)
                    .attr('r', radius)
                    .attr('fill', fill);
            });
    }
}
