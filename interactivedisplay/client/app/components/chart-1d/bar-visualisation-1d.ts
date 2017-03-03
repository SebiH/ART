import { HtmlChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';
import { ChartVisualisation1d } from './chart-visualisation-1d';

import * as d3 from 'd3';
import * as _ from 'lodash';

export class BarVisualisation1d extends ChartVisualisation1d {

    private dataContainer: HtmlChartElement;
    private data: { category: string, amount: number }[] = [];
    private domain: [number, number] = [0, 0];
    private categories: string[] = [];

    public constructor(public dimension: ChartDimension) {
        super();

        let tempData = {};
        let maxDomain = 0;
        for (let category of dimension.data) {
            if (tempData[category] !== undefined) {
                tempData[category] += 1;
                maxDomain = Math.max(maxDomain, tempData[category]);
            } else {
                tempData[category] = 0;
            }
        }

        this.domain = [0, maxDomain];

        for (let mapping of dimension.mappings) {
            this.data.push({
                category: mapping.name,
                amount: tempData[mapping.value] || 0
            });
            this.categories.push(mapping.name);
        }
    }

    public register(root: HtmlChartElement, width: number, height: number): void {
        this.dataContainer = root.append('g');

        let x = d3.scaleLinear().rangeRound([width, 0]).domain(this.domain);
        let y = d3.scaleBand().rangeRound([0, height + 2 /* -> layout offset.. */]).domain(this.categories);

        this.dataContainer.selectAll('.bar')
            .data(this.data)
            .enter().append('rect')
                .attr('class', 'bar')
                .attr('fill', (d, i) => this.getColor(d.category))
                .attr('x', d => x(d.amount))
                .attr('y', d => y(d.category))
                .attr('height', y.bandwidth())
                .attr('width', d => width - x(d.amount));
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
}
