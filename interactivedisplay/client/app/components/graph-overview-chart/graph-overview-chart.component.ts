import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Graph, ChartDimension, Filter, FilterType } from '../../models/index';
import { GraphDataProvider, FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

import * as _ from 'lodash';

@Component({
    selector: 'graph-overview-chart',
    template: `<chart-1d [height]="height"
                         [width]="width"
                         [dimension]="graph.isFlipped ? dimY : dimX"

                         touch-button
                         (touchclick)="onClick($event)"

                         moveable
                         (moveStart)="onMoveStart($event)"
                         (moveUpdate)="onMoveUpdate($event)"
                         (moveEnd)="onMoveEnd($event)">
               </chart-1d>`
})
export class GraphOverviewChartComponent implements OnInit, OnDestroy {
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

    ngOnInit() {
        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('dimX') >= 0 || changes.indexOf('dimY') >= 0)
            .subscribe(changes => this.updateDimensions(this.graph.dimX, this.graph.dimY));

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isFlipped') >= 0)
            .subscribe(changes => this.switchFilter());

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
                            this.updateFilter();
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
                            this.updateFilter();
                        }
                    });
            }
        } else {
            this.dimY = null;
        }

        this.updateFilter();
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

    private categoryClick(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.flipCategory(clickedCategory);

        if (this.filters.length === 0) {
            this.categoryUpdateFilters();
        }
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
        if (this.filters.length === 0) {
            this.categoryUpdateFilters();
        }
    }

    private categoryUpdateFilters(): void {
        let dim = this.getOverviewDimension();

        if (dim) {
            if (this.filters.length == 0) {
                // no categorical filters => all categories are active
                for (let mapping of dim.mappings) {
                    this.addCategoryFilter(mapping.value);
                }
            }

            for (let mapping of dim.mappings) {
                let filter = _.find(this.filters, f => f.category == mapping.value);
                this.chart.setCategoryActive(mapping.value, filter != null);
            }
        }
    }


    private flipCategory(category: number): void {
        let dim = this.getOverviewDimension();

        if (dim && _.find(dim.mappings, m => m.value == category)) {
            let filter = _.find(this.filters, f => f.category == category);

            if (filter) {
                this.chart.setCategoryActive(category, false);
                _.pull(this.filters, filter);
                this.filterProvider.removeFilter(filter);

            } else {
                this.addCategoryFilter(category);
                this.chart.setCategoryActive(category, true);
            }
        }
    }

    private addCategoryFilter(category: number): void {
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

    private lineClick(eveny: any): void {
        // TODO: Check if click was inside filter, offer delete dialog?
    }

    private lineMoveStart(event: any): void {

    }


    private lineMoveUpdate(event: any): void {

    }

    private lineMoveEnd(event: any): void {

    }

    private lineUpdateFilters(): void {
        
    }

    private initMetricFilters(): void {

    }
}
