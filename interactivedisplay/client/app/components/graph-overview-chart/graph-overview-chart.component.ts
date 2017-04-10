import { Component, Input } from '@angular/core';
import { Graph, ChartDimension } from '../../models/index';

import * as _ from 'lodash';

@Component({
    selector: 'graph-overview-chart',
    template: `
    <div *ngIf="dim && !dim.isMetric">
        <category-overview-chart
            [width]="width" [height]="height"
            [graph]="graph"
            [dim]="dim">
        </category-overview-chart>
    </div>
    <div *ngIf="dim && dim.isMetric">
        <metric-overview-chart
            [width]="width" [height]="height"
            [graph]="graph"
            [dim]="dim">
        </metric-overview-chart>
    </div>
    `
})
export class GraphOverviewChartComponent implements AfterViewInit, OnDestroy, OnChanges {
    @Input() graph: Graph;
    @Input() width: number;
    @Input() height: number;
    @Input() dim: ChartDimension = null;

    @ViewChild(Chart1dComponent) chart: Chart1dComponent;

    private isActive: boolean = true;
    private clearOnSwitch: boolean = false;
    private has2dFilter: boolean = false;

    private readonly filters: Filter[] = [];

    constructor(private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isColored') >= 0)
            .subscribe(changes => this.colorUpdate());

        this.filterProvider.getFilters()
            .first()
            .subscribe(filters => this.initFilters(filters));
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['dim']) {
            setTimeout(() => this.switchFilter());
        }
    }

    private initFilters(allFilters: Filter[]): void {
        this.has2dFilter = false;
        let dimType = this.graph.isFlipped ? 'y' : 'x';

        for (let filter of allFilters) {
            if (filter.origin.id == this.graph.id && filter.dimType == dimType) {
                if (filter.isOverview) {
                    this.filters.push(filter);
                } else {
                    this.has2dFilter = true;
                }
            }
        }

        if (this.dim) {
            if (this.dim.isMetric) {
                this.lineUpdateFilters();
            } else {
                this.categoryUpdateFilters();
            }
        }
    }

    private updateFilter(): void {
        if (this.dim) {
            if (this.dim.isMetric) {
                this.lineUpdateFilters();
            } else {
                this.categoryUpdateFilters();
            }
        }
    }

    private switchFilter(): void {
        if (this.invisibleColorFilter !== null) {
            this.filterProvider.removeFilter(this.invisibleColorFilter);
            this.invisibleColorFilter = null;
        }

        this.updateFilter();
    }

    private colorUpdate(): void {
    }

    private onClick(event: any): void {
    }


    private onMoveStart(event: any): void {
    }

    private onMoveUpdate(event: any): void {
    }

    private onMoveEnd(event: any): void {
    }









    /**
     *    line handling
     */

    private deleteButtonFilter: Filter = null;
    private deleteButtonStyle = { 'top': 0 };

    private lineClick(event: any): void {
        if (this.deleteButtonFilter == null) {
            let buttonHeight = 50;
            this.deleteButtonStyle.top = event.relativePos.y - buttonHeight / 2;

            let clickedData = this.chart.invert(event.relativePos.y);
            for (let filter of this.filters) {
                if (filter.range[0] <= clickedData && clickedData <= filter.range[1]) {
                    this.deleteButtonFilter = filter;
                    break;
                }
            }

        } else {
            this.deleteButtonFilter = null;
        }

    }

    private deleteButtonClick(event: any): void {
        if (this.deleteButtonFilter != null) {
            _.pull(this.filters, this.deleteButtonFilter);
            this.filterProvider.removeFilter(this.deleteButtonFilter);
            this.deleteButtonFilter = null;
            this.lineUpdateFilters();
        }
    }



    private currentFilter: Filter = null;

    private lineMoveStart(event: any): void {
        let selectedData = this.chart.invert(event.relativePos.y);
        this.currentFilter = this.createLineFilter([selectedData, selectedData]);
        this.filters.push(this.currentFilter);
    }


    private lineMoveUpdate(event: any): void {
        let selectedData = this.chart.invert(event.relativePos.y);
        this.currentFilter.range[1] = selectedData;
        this.filterProvider.updateFilter(this.currentFilter);

        this.lineUpdateFilters();
    }

    private lineMoveEnd(event: any): void {
        if (this.currentFilter.range[1] < this.currentFilter.range[0]) {
            let temp = this.currentFilter.range[0];
            this.currentFilter.range[0] = this.currentFilter.range[1];
            this.currentFilter.range[1] = temp;
        }


        // remove filters that are *too* small, probably accidentally created
        let start = this.currentFilter.range[0];
        let end = this.currentFilter.range[1];
        let percent = Math.abs(end - start) / (this.dim.domain.max - this.dim.domain.min);

        if (percent < 0.01) {
            this.filterProvider.removeFilter(this.currentFilter);
            _.pull(this.filters, this.currentFilter);
        } else {
            this.filterProvider.updateFilter(this.currentFilter);
        }

        this.mergeFilters();

        this.currentFilter = null;
        this.lineUpdateFilters();
    }

    private mergeFilters(): void {
        for (let i = 0; i < this.filters.length; i++) {
            let f1 = this.filters[i];

            for (let j = this.filters.length - 1; j >= 0; j--) {
                if (i == j) {
                    continue;
                }

                let f2 = this.filters[j];

                let isMinContained = (f1.range[0] <= f2.range[0] && f2.range[0] <= f1.range[1]);
                let isMaxContained = (f1.range[0] <= f2.range[1] && f2.range[1] <= f1.range[1]);

                if (isMinContained || isMaxContained) {
                    f1.range[0] = Math.min(f1.range[0], f2.range[0]);
                    f1.range[1] = Math.max(f1.range[1], f2.range[1]);

                    _.pull(this.filters, f2);
                    this.filterProvider.removeFilter(f2);
                    this.filterProvider.updateFilter(f1);

                } else if (f1.range[0] <= f2.range[0] && f2.range[1] <= f1.range[1]) {
                    // f2 is contained within f1
                    _.pull(this.filters, f2);
                    this.filterProvider.removeFilter(f2);
                }
            }
        }

        for (let i = this.filters.length - 1; i >= 0; i--) {
            let filter = this.filters[i];
            if (filter.range[0] == filter.range[1]) {
                _.pull(this.filters, filter);
                this.filterProvider.removeFilter(filter);
            }
        }
    }

    private lineUpdateFilters(): void {
        let ranges: [number, number][] = [];

        for (let filter of this.filters) {
            if (filter.range && filter.range.length >= 2) {
                let start = filter.range[0];
                let end = filter.range[1];

                if (start < end) {
                    ranges.push([start, end]);
                } else {
                    ranges.push([end, start]);
                }
            } else {
                // TODO: delete filter?
                console.warn('invalid filter!');
                console.warn(filter);
            }
        }

        this.deleteButtonFilter = null;

        this.chart.setHighlightedRanges(ranges);
        this.lineColorUpdate();
    }

    private invisibleColorFilter: Filter = null;

    private lineColorUpdate(): void {
        if (this.dim) {
            if (this.graph.isColored) {
                if (this.filters.length == 0 && this.invisibleColorFilter === null && !this.has2dFilter) {
                    this.invisibleColorFilter = this.createLineFilter([this.dim.domain.min, this.dim.domain.max]);
                }
            } else {
                if (this.invisibleColorFilter != null) {
                    this.filterProvider.removeFilter(this.invisibleColorFilter);
                    this.invisibleColorFilter = null;
                }
            }
        }
    }


    private createLineFilter(range: [number, number]): Filter {
        if (this.invisibleColorFilter !== null) {
            this.filterProvider.removeFilter(this.invisibleColorFilter);
            this.invisibleColorFilter = null;
        }

        let filter = this.filterProvider.createFilter(this.graph);

        filter.isOverview = true;
        filter.dimType = this.graph.isFlipped ? 'y' : 'x';
        filter.type = FilterType.Metric;
        filter.range = range;

        this.filterProvider.updateFilter(filter);

        return filter;
    }
}
