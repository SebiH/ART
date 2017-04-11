import { Component, Input, AfterViewInit, OnDestroy, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { Graph, Filter, CategoryFilter, ChartDimension } from '../../models/index';
import { FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

import * as _ from 'lodash';

@Component({
    selector: 'category-overview-chart',
    templateUrl: `
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

    constructor(private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.filterProvider.getFilters()
            .takeWhile(() => this.isActive)
            .subscribe((filters) => {
                this.filters = filters;
                this.draw();
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isColored') >= 0)
            .subscribe(changes => this.colorUpdate());
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

    private draw(forceActive: boolean = false) {
        let axis = this.graph.isFlipped ? 'y' : 'x';

        for (let mapping of this.dim.mappings) {
            this.chart.setCategoryActive(mapping.value, forceActive);
        }

        for (let filter of this.filters) {
            let catFilter = filter as CategoryFilter;
            if (filter && filter.origin.id == this.graph.id && filter.boundDimensions == axis) {
                this.chart.setCategoryActive(catFilter.category, true);
            }
        }
    }
    


    private flippedCategories: number[] = [];
    // stop automatic creation of filters when
    private hasNoFilters: boolean = false;


    private onClick(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.flipCategory(clickedCategory);
    }


    private onMoveStart(event: any): void {
        this.flippedCategories = [];

        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.flipCategory(clickedCategory);
        this.flippedCategories.push(clickedCategory);
    }

    private onMoveUpdate(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        if (this.flippedCategories.indexOf(clickedCategory) < 0) {
            this.flipCategory(clickedCategory);
            this.flippedCategories.push(clickedCategory);
        }
    }

    private onMoveEnd(event: any): void {
    }


    private colorUpdate(): void {
        let activeFilters = this.getActiveFilters();       

        if (activeFilters.length == this.dim.mappings.length && !this.graph.isColored) {
            // all categories are active -> remove all filters
            while (activeFilters.length > 0) {
                this.filterProvider.removeFilter(activeFilters.pop());
            }
        }

        let hasDetailFilter = _.find(this.filters, f => 
            f.boundDimensions == 'xy' && f.origin.id == this.graph.id
        ) != null;

        if (this.filters.length == 0 && this.graph.isColored && !hasDetailFilter) {
            for (let mapping of this.dim.mappings) {
                this.addCategoryFilter(mapping.value, mapping.color);
            }

            this.draw();
            this.hasNoFilters = false;
        }
    }



    private flipCategory(category: number): void {
        let mapping = _.find(this.dim ? this.dim.mappings : [], m => m.value == category);
        let activeFilters = this.getActiveFilters();

        if (mapping) {

            if (activeFilters.length == 0 && !this.hasNoFilters) {
                // no categorical filters => all categories are active
                for (let mapping of this.dim.mappings) {
                    this.addCategoryFilter(mapping.value, mapping.color);
                }
            }

            let filter = _.find(activeFilters, f => f.category == category);

            if (filter) {
                this.filterProvider.removeFilter(filter);
                this.hasNoFilters = (activeFilters.length === 0);

            } else {
                this.addCategoryFilter(category, mapping.color);
                this.hasNoFilters = false;
            }

            if (this.filters.length == this.dim.mappings.length && !this.graph.isColored) {
                // all categories are active -> remove all filters
                while (activeFilters.length > 0) {
                    this.filterProvider.removeFilter(activeFilters.pop());
                }
                this.draw(true);
            } else {
                this.draw();
            }

        }
    }


    private addCategoryFilter(category: number, color: string): void {
        let filter = this.filterProvider.createCategoryFilter(this.graph);
        filter.boundDimensions = this.graph.isFlipped ? 'y' : 'x';
        filter.color = color;
        filter.category = category;
    }
}
