import { Component, Input, AfterViewInit, OnDestroy, ViewChild } from '@angular/core';
import { Graph, ChartDimension, Filter, FilterType } from '../../models/index';
import { GraphDataProvider, FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

import * as _ from 'lodash';

@Component({
    selector: 'graph-overview-chart',
    templateUrl: './app/components/graph-overview-chart/graph-overview-chart.html',
    styleUrls: [ './app/components/graph-overview-chart/graph-overview-chart.css' ]
})
export class GraphOverviewChartComponent implements AfterViewInit, OnDestroy {
    @Input() graph: Graph;
    @Input() width: number;
    @Input() height: number;

    @ViewChild(Chart1dComponent) chart: Chart1dComponent;

    private isActive: boolean = true;
    private dimX: ChartDimension = null;
    private dimY: ChartDimension = null;

    private readonly filters: Filter[] = [];

    constructor(
        private dataProvider: GraphDataProvider,
        private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('dimX') >= 0 || changes.indexOf('dimY') >= 0)
            .subscribe(changes => this.updateDimensions(this.graph.dimX, this.graph.dimY));

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isFlipped') >= 0)
            .subscribe(changes => this.switchFilter());

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isColored') >= 0)
            .subscribe(changes => this.colorUpdate());

        this.filterProvider.getFilters()
            .first()
            .subscribe(filters => this.initFilters(filters));

        this.updateDimensions(this.graph.dimX, this.graph.dimY);
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private updateDimensions(newDimX: string, newDimY: string): void {
        if (newDimX) {
            if (this.dimX === null || this.dimX.name !== newDimX) {
                this.dataProvider.getData(newDimX)
                    .first()
                    .subscribe(data => {
                        // just in case it has changed in the meantime
                        if (newDimX === this.graph.dimX) {
                            this.dimX = data;
                            setTimeout(() => this.updateFilter());
                        }
                    });
            }
        } else {
            this.dimX = null;
        }

        if (newDimY) {
            if (this.dimY === null || this.dimY.name !== newDimY) {
                this.dataProvider.getData(newDimY)
                    .first()
                    .subscribe(data => {
                        // just in case it has changed in the meantime
                        if (newDimY === this.graph.dimY) {
                            this.dimY = data;
                            setTimeout(() => this.updateFilter());
                        }
                    });
            }
        } else {
            this.dimY = null;
        }

        setTimeout(() => this.updateFilter());
    }

    private initFilters(allFilters: Filter[]): void {
        for (let filter of allFilters) {
            if (filter.origin.id == this.graph.id && filter.isOverview) {
                this.filters.push(filter);
            }
        }

        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineUpdateFilters();
            } else {
                this.categoryUpdateFilters();
            }
        }
    }

    private updateFilter(): void {
        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineUpdateFilters();
            } else {
                this.categoryUpdateFilters();
            }
        }
    }

    private switchFilter(): void {
        while (this.filters.length > 0) {
            let filter = this.filters.pop();
            this.filterProvider.removeFilter(filter);
        }

        this.updateFilter();
    }

    private colorUpdate(): void {
        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineColorUpdate();
            } else {
                this.categoryColorUpdate();
            }
        }
    }


    private getOverviewDimension(): ChartDimension {
        return this.graph.isFlipped ? this.dimY : this.dimX;
    }


    private onClick(event: any): void {
        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineClick(event);
            } else {
                this.categoryClick(event);
            }
        }
    }


    private onMoveStart(event: any): void {
        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineMoveStart(event);
            } else {
                this.categoryMoveStart(event);
            }
        }
    }

    private onMoveUpdate(event: any): void {
        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineMoveUpdate(event);
            } else {
                this.categoryMoveUpdate(event);
            }
        }
    }

    private onMoveEnd(event: any): void {
        let dim = this.getOverviewDimension();
        if (dim) {
            if (dim.isMetric) {
                this.lineMoveEnd(event);
            } else {
                this.categoryMoveEnd(event);
            }
        }
    }






    /**
     *    Category handling
     */

    private flippedCategories: number[] = [];
    private hasNoFilters: boolean = false;

    private categoryClick(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.flipCategory(clickedCategory);
    }

    private categoryMoveStart(event: any): void {
        this.flippedCategories = [];

        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.flipCategory(clickedCategory);
        this.flippedCategories.push(clickedCategory);
    }


    private categoryMoveUpdate(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        if (this.flippedCategories.indexOf(clickedCategory) < 0) {
            this.flipCategory(clickedCategory);
            this.flippedCategories.push(clickedCategory);
        }
    }

    private categoryMoveEnd(event: any): void {
    }

    private categoryUpdateFilters(): void {
        let dim = this.getOverviewDimension();

        if (dim && this.filters.length > 0) {
            for (let mapping of dim.mappings) {
                let filter = _.find(this.filters, f => f.category == mapping.value);
                this.chart.setCategoryActive(mapping.value, filter != null);
            }
        }
    }

    private categoryColorUpdate(): void {
        let dim = this.getOverviewDimension();

        if (dim) {
            if (this.filters.length == dim.mappings.length && !this.graph.isColored) {
                // all categories are active -> remove all filters
                while (this.filters.length > 0) {
                    this.filterProvider.removeFilter(this.filters.pop());
                }
            }

            if (this.filters.length == 0 && this.graph.isColored) {
                for (let mapping of dim.mappings) {
                    this.addCategoryFilter(mapping.value, mapping.color);
                    this.chart.setCategoryActive(mapping.value, true);
                }

                this.hasNoFilters = false;
            }           
        }
    }


    private flipCategory(category: number): void {
        let dim = this.getOverviewDimension();
        let mapping = _.find(dim ? dim.mappings : [], m => m.value == category);

        if (mapping) {

            if (this.filters.length == 0 && !this.hasNoFilters) {
                // no categorical filters => all categories are active
                for (let mapping of dim.mappings) {
                    this.addCategoryFilter(mapping.value, mapping.color);
                }
            }

            let filter = _.find(this.filters, f => f.category == category);

            if (filter) {
                this.chart.setCategoryActive(category, false);
                _.pull(this.filters, filter);
                this.filterProvider.removeFilter(filter);
                this.hasNoFilters = (this.filters.length === 0);

            } else {
                this.addCategoryFilter(category, mapping.color);
                this.chart.setCategoryActive(category, true);
                this.hasNoFilters = false;
            }

            if (this.filters.length == dim.mappings.length && !this.graph.isColored) {
                // all categories are active -> remove all filters
                while (this.filters.length > 0) {
                    this.filterProvider.removeFilter(this.filters.pop());
                }
            }
        }
    }

    private addCategoryFilter(category: number, color: string): void {
        let filter = this.filterProvider.createFilter(this.graph);
        filter.isOverview = true;
        filter.type = FilterType.Categorical;
        filter.category = category;
        this.filterProvider.updateFilter(filter);
        this.filters.push(filter);
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

            let clickedData = this.chart.convertData(event.relativePos.y);
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
        this.currentFilter = this.filterProvider.createFilter(this.graph);
        this.currentFilter.isOverview = true;
        this.currentFilter.type = FilterType.Line;
        let selectedData = this.chart.convertData(event.relativePos.y);
        this.currentFilter.range = [ selectedData, selectedData ];

        this.filters.push(this.currentFilter);
        this.filterProvider.updateFilter(this.currentFilter);
    }


    private lineMoveUpdate(event: any): void {
        let selectedData = this.chart.convertData(event.relativePos.y);

        if (selectedData < this.currentFilter.range[0]) {
            this.currentFilter.range[0] = selectedData;
        } else {
            this.currentFilter.range[1] = selectedData;
        }

        this.lineUpdateFilters();
    }

    private lineMoveEnd(event: any): void {
        if (Math.abs(this.currentFilter.range[0] - this.currentFilter.range[1]) < 0.1) {
            this.filterProvider.removeFilter(this.currentFilter);
            _.pull(this.filters, this.currentFilter);
        }

        this.currentFilter = null;
    }

    private lineUpdateFilters(): void {
        let ranges: [number, number][] = [];

        for (let filter of this.filters) {
            let start = this.chart.invertData(filter.range[0]);
            let end = this.chart.invertData(filter.range[1]);

            if (start < end) {
                ranges.push([start, end]);
            } else {
                ranges.push([end, start]);
            }
        }

        this.deleteButtonFilter = null;

        this.chart.setHighlightedRanges(ranges);
    }

    private lineColorUpdate(): void {

    }
}
