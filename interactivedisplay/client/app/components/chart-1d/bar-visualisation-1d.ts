import { HtmlChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';
import { ChartVisualisation1d } from './chart-visualisation-1d';

import * as d3 from 'd3';
import * as _ from 'lodash';

const TEXT_Y_OFFSET = 34/2; // css font height/2
const COL_INACTIVE = "#BDBDBD"; // gray 400

export class BarVisualisation1d extends ChartVisualisation1d {

    private dataContainer: HtmlChartElement;
    private data: { category: string, amount: number, isActive: boolean, categoryValue: number }[] = [];
    private yScale: d3.ScaleBand<string> = null;

    public constructor(public dimension: ChartDimension) {
        super();

        let tempData = {};
        for (let category of dimension.data) {
            if (tempData[category.value] === undefined) {
                tempData[category.value] = 1;
            } else {
                tempData[category.value] += 1;
            }
        }

        for (let mapping of dimension.mappings) {
            this.data.push({
                category: mapping.name,
                categoryValue: mapping.value,
                amount: tempData[mapping.value] || 0,
                isActive: true
            });
        }
    }


    public register(root: HtmlChartElement, width: number, height: number): void {
        this.dataContainer = root.append('g');
        let categories = <string[]>_.map(this.data, 'category');

        let x = d3.scalePow().exponent(0.5).rangeRound([0, width]).domain([0, _.maxBy(this.data, 'amount').amount * 1.1]);
        let y = d3.scaleBand().rangeRound([0, height]).domain(categories);
        this.yScale = y;

        this.dataContainer.selectAll('.bar')
            .data(this.data)
            .enter().append('rect')
                .attr('class', 'bar')
                .attr('fill', (d, i) => this.getColor(d.category))
                .attr('x', 0)
                .attr('y', d => y(d.category))
                .attr('height', y.bandwidth())
                .attr('width', d => x(d.amount));

        for (let category of categories) {
            let barWidth = y.step();
            let barHeight = y(category);
            this.dataContainer.append('text')
                .text(category)
                .attr('class', 'tick-label bar-tick-label')
                .attr('text-anchor', 'middle')
                .attr('x', width / 2)
                .attr('y', barHeight + barWidth / 2);
            }
    }


    public unregister(): void {
        this.dataContainer.remove();
    }


    public resize(width: number, height: number): void {
        // TODO.
    }


    private getColor(category: string): string {
        for (let mapping of this.dimension.mappings) {
            if (mapping.name == category) {
                return mapping.color;
            }
        }

        return '#00ff00';
    }

    public invert(val: number): number {
        let barWidth = this.yScale.step();
        let index = Math.floor(val / barWidth);

        if (this.data[index]) {
            return this.data[index].categoryValue;
        }

        return -1;
    }

    public setCategoryActive(category: number, isActive: boolean): void {
        let entry = _.find(this.data, d => d.categoryValue == category);

        if (entry) {
            entry.isActive = isActive;

            this.dataContainer.selectAll('.bar')
                .transition()
                .duration(100)
                .attr('fill', (d: any) => d.isActive ? this.getColor(d.category) : COL_INACTIVE);
        }
    }
}
