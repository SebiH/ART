import { Component, Input, AfterViewInit, OnDestroy, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { Graph, Filter, CategoryFilter, ChartDimension } from '../../models/index';
import { FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

import * as _ from 'lodash';

@Component({
    selector: 'category-overview-chart',
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
</div>
`,
    styleUrls: [ './app/components/graph-overview-chart/graph-overview-chart.css' ]
})
export class CategoryOverviewChartComponent implements AfterViewInit, OnDestroy, OnChanges {
    @Input() graph: Graph;
    @Input() width: number;
    @Input() height: number;
    @Input() dim: ChartDimension = null;

    @ViewChild(Chart1dComponent) chart: Chart1dComponent;

    private isActive: boolean = true;
    private filters: Filter[] = [];
    private userDeletedAllFilters: boolean = false;

    constructor(private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.filterProvider.getFilters()
            .takeWhile(() => this.isActive)
            .subscribe((filters) => {
                this.filters = filters;
                setTimeout(() => this.draw());
            });
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    ngOnChanges(changes: SimpleChanges) {
        // let chart update first
        if (changes['dim']) {
            this.userDeletedAllFilters = false;
            setTimeout(() => this.draw());
        }
    }

    private getActiveFilters(): CategoryFilter[] {
        let axis = this.graph.isFlipped ? 'y' : 'x';
        let filters = _.filter(this.filters, filter => {
            let cf = filter as CategoryFilter;
            return cf &&
                cf.origin.id == this.graph.id &&
                cf.boundDimensions == axis;
        });

        return _.map(filters, f => f as CategoryFilter);
    }

    private draw() {
        let filters = this.getActiveFilters();

        if (filters.length === 0 && !this.userDeletedAllFilters) {
            for (let mapping of this.dim.mappings) {
                this.chart.setCategoryActive(mapping.value, true);
            }
        } else {
            for (let mapping of this.dim.mappings) {
                this.chart.setCategoryActive(mapping.value, !!_.find(filters, f => f.category == mapping.value));
            }
        }
    }
    


    private flippedCategories: number[] = [];

    private onClick(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.toggleCategory(clickedCategory);
    }


    private onMoveStart(event: any): void {
        this.flippedCategories = [];

        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.toggleCategory(clickedCategory);
        this.flippedCategories.push(clickedCategory);
    }

    private onMoveUpdate(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        if (this.flippedCategories.indexOf(clickedCategory) < 0) {
            this.toggleCategory(clickedCategory);
            this.flippedCategories.push(clickedCategory);
        }
    }

    private onMoveEnd(event: any): void {
    }


    private toggleCategory(category: number): void {
        let mapping = _.find(this.dim.mappings, m => m.value == category);

        if (mapping) {

            if (this.getActiveFilters().length == 0 && !this.userDeletedAllFilters) {
                // no categorical filters => all categories are active
                for (let mapping of this.dim.mappings) {
                    this.addCategoryFilter(mapping.value, mapping.color);
                }
            }

            for (let filter of this.getActiveFilters()) {
                filter.isUserGenerated = true;
            }

            let filter = _.find(this.getActiveFilters(), f => f.category == category);

            if (filter) {
                this.filterProvider.removeFilter(filter);
                this.userDeletedAllFilters = (this.getActiveFilters().length === 0);

            } else {
                this.addCategoryFilter(category, mapping.color);
                this.userDeletedAllFilters = false;
            }

            let filters = this.getActiveFilters();
            let isColored = (this.graph.isFlipped ? this.graph.useColorY : this.graph.useColorX);
            if (filters.length == this.dim.mappings.length && !isColored) {
                // all categories are active -> remove all filters
                while (filters.length > 0) {
                    this.filterProvider.removeFilter(filters.pop());
                }
            }

            this.draw();
        }
    }


    private addCategoryFilter(category: number, color: string): void {
        let filter = this.filterProvider.createCategoryFilter(this.graph);
        filter.boundDimensions = this.graph.isFlipped ? 'y' : 'x';
        filter.color = color;
        filter.category = category;
        filter.isUserGenerated = true;
    }
}
