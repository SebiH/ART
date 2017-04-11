import { Component, Input, AfterViewInit, OnDestroy, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { Graph, Filter, MetricFilter, ChartDimension } from '../../models/index';
import { FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

import * as _ from 'lodash';

@Component({
    selector: 'metric-overview-chart',
    template: `
<div class="root" [style.height.px]="height">
    <chart-1d class="chart"
            [height]="height"
            [width]="width"
            [dimension]="dim">
    </chart-1d>

    <div class="chart-overlay"
        touch-button
        (touchclick)="onClick($event)"

        moveable
        (moveStart)="onMoveStart($event)"
        (moveUpdate)="onMoveUpdate($event)"
        (moveEnd)="onMoveEnd($event)">
    </div>

    <div class="delete-button"
        [ngStyle]="deleteButtonStyle"
        *ngIf="deleteButtonFilter"

        touch-button
        (touchclick)="deleteButtonClick($event)">
        Delete
    </div>
</div>
`,
    styleUrls: [ './app/components/graph-overview-chart/graph-overview-chart.css' ]
})
export class MetricOverviewChartComponent implements AfterViewInit, OnDestroy, OnChanges {
    @Input() graph: Graph;
    @Input() width: number;
    @Input() height: number;
    @Input() dim: ChartDimension = null;

    @ViewChild(Chart1dComponent) chart: Chart1dComponent;

    private isActive: boolean = true;
    private filters: Filter[] = [];

    constructor(private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.filterProvider.getFilters()
            .takeWhile(() => this.isActive)
            .subscribe((filters) => {
                this.filters = filters;
                this.draw();
            });
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    ngOnChanges(changes: SimpleChanges) {
        // let chart update first
        if (changes['dim']) {
            setTimeout(() => this.draw());
        }
    }

    private getActiveFilters(): MetricFilter[] {
        let axis = this.graph.isFlipped ? 'y' : 'x';
        let filters = _.filter(this.filters, filter => {
            let cf = filter as MetricFilter;
            return cf &&
                cf.origin.id == this.graph.id &&
                cf.boundDimensions == axis;
        });

        return _.map(filters, f => f as MetricFilter);
    }


    private createFilter(range: [number, number]): MetricFilter {
        let activeFilters = this.getActiveFilters();
        for (let filter of activeFilters) {
            if (!filter.isUserGenerated) {
                this.filterProvider.removeFilter(filter);
            }
        }

        let filter = this.filterProvider.createMetricFilter(this.graph);
        filter.isUserGenerated = true;
        filter.boundDimensions = this.graph.isFlipped ? 'y' : 'x';
        filter.gradient = this.dim.gradient;
        filter.setRange(range);

        return filter;
    }


    private draw(): void {
        let ranges: [number, number][] = [];

        for (let filter of this.getActiveFilters()) {
            ranges.push([filter.range.min, filter.range.max]);
        }

        this.chart.setHighlightedRanges(ranges);
    }



    private deleteButtonFilter: Filter = null;
    private deleteButtonStyle = { 'top': 0 };


    private onClick(event: any): void {
        if (this.deleteButtonFilter === null) {
            let buttonHeight= 50;
            this.deleteButtonStyle.top = event.relativePos.y - buttonHeight / 2;

            let clickedData = this.chart.invert(event.relativePos.y);
            for (let filter of this.getActiveFilters()) {
                if (filter.range.min <= clickedData && clickedData <= filter.range.max) {
                    this.deleteButtonFilter = filter;
                    break;
                }
            }

        } else {
            this.deleteButtonFilter = null;
        }
    }

    private deleteButtonClick(event: any): void {
        if (this.deleteButtonFilter !== null) {
            this.filterProvider.removeFilter(this.deleteButtonFilter);
            this.deleteButtonFilter = null;
        }
    }



    private activeFilter: MetricFilter = null;
    private startPoint: number = 0;

    private onMoveStart(event: any): void {
        this.mergeFilters();
        this.startPoint = this.chart.invert(event.relativePos.y);
        this.activeFilter = this.createFilter([this.startPoint, this.startPoint]);
        this.draw();
    }

    private onMoveUpdate(event: any): void {
        if (this.activeFilter != null) {
            let endPoint = this.chart.invert(event.relativePos.y);
            this.activeFilter.setRange([this.startPoint, endPoint]);
        }

        this.draw();
    }

    private onMoveEnd(event: any): void {
        if (this.activeFilter != null) {
            let endPoint = this.chart.invert(event.relativePos.y);
            this.activeFilter.setRange([this.startPoint, endPoint]);


            // delete filters that are too small (probably created on accident)
            let percent = Math.abs(endPoint - this.startPoint) / (this.dim.domain.max - this.dim.domain.min);
            if (percent < 0.01) {
                this.filterProvider.removeFilter(this.activeFilter);
            }

            this.activeFilter = null;

            this.mergeFilters();
        }

        this.draw();
    }


    private mergeFilters(): void {
        let activeFilters = this.getActiveFilters();
        for (let i = 0; i < activeFilters.length; i++) {
            let f1 = activeFilters[i];

            for (let j = activeFilters.length - 1; j >= 0; j--) {
                if (i == j) {
                    continue;
                }

                let f2 = activeFilters[j];

                let isMinContained = (f1.range.min <= f2.range.min && f2.range.min <= f1.range.max);
                let isMaxContained = (f1.range.min <= f2.range.max && f2.range.max <= f1.range.max);

                if (isMinContained || isMaxContained) {
                    let start = Math.min(f1.range.min, f2.range.min);
                    let end = Math.max(f1.range.max, f2.range.max);
                    f1.setRange([start, end]);

                    _.pull(activeFilters, f2);
                    this.filterProvider.removeFilter(f2);

                } else if (f1.range.min <= f2.range.min && f2.range.max <= f1.range.max) {
                    // f2 is contained within f1
                    _.pull(activeFilters, f2);
                    this.filterProvider.removeFilter(f2);
                }
            }
        }

        for (let i = activeFilters.length - 1; i >= 0; i--) {
            let filter = activeFilters[i];
            if (filter.range.min == filter.range.max) {
                _.pull(this.filters, filter);
                this.filterProvider.removeFilter(filter);
            }
        }
    }
}
