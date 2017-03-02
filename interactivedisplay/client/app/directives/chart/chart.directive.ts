import { Directive, OnInit, OnChanges, SimpleChanges, ElementRef, Input } from '@angular/core';
import { } from '../../models/index';

import * as d3 from 'd3';
import * as _ from 'lodash';

import { ChartElement } from './chart-element';
import { HtmlChartElement } from './html-chart-element';

@Directive({
    selector: '[chart]'
})
export class ChartDirective implements OnInit, OnChanges {

    @Input() width: number = 600;
    @Input() height: number = 600;
    @Input() margin = { top: 50, right: 50, bottom: 100, left: 100 };

    private svgElement: HtmlChartElement;
    private chartContainer: HtmlChartElement;
    private chartElements: ChartElement[] = []

    constructor(private elementRef: ElementRef) { }

    ngOnInit(): void {
        this.initialize();
    }

    ngOnChanges(changes: SimpleChanges): void {

    }

    public addElement(element: ChartElement): void {
        element.register(this.chartContainer, this.width, this.height);
        this.chartElements.push(element);
    }

    public removeElement(element: ChartElement): void {
        _.pull(this.chartElements, element);
        element.unregister();
    }


    private getTotalSize() {
        return {
            width: this.width + this.margin.left + this.margin.right,
            height: this.height + this.margin.top + this.margin.bottom
        }
    }

    private initialize(): void {
        let d3element = d3.select(this.elementRef.nativeElement);
        d3element.html('');

        let totalSize = this.getTotalSize();
        this.svgElement = d3element.append('svg')
            .attr('width', totalSize.width)
            .attr('height', totalSize.height);
        this.chartContainer = this.svgElement
            .append('g')
            .attr('transform', 'translate(' + this.margin.left + ',' + this.margin.top + ')');
    }

    private resize(): void {
        var totalSize = this.getTotalSize();
        this.svgElement
            .attr('width', totalSize.width)
            .attr('height', totalSize.height);
        this.chartContainer
            .attr('transform', 'translate(' + this.margin.left + ',' + this.margin.top + ')');

        for (let element of this.chartElements) {
            element.resize(this.width, this.height);
        }
    }
}
