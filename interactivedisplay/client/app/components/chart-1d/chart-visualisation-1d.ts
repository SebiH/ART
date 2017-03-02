import { HtmlChartElement, ChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';

import * as d3 from 'd3';
import * as _ from 'lodash';

const FILL_COLOURS = [
    '#E53935', // red
    '#1E88E5', // blue
    '#43A047', // green
    '#FDD835', // yellow
    '#8E24AA', // purple
    '#F4511E', // orange
    '#00897B', // teal
];

export class ChartVisualisation1d extends ChartElement {

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
                .attr('fill', (d, i) => FILL_COLOURS[i])
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

}
