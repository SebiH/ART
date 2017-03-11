import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Graph, ChartDimension, Filter } from '../../models/index';
import { GraphDataProvider, FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

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
            .filter(changes => changes.indexOf('isColored') >= 0)
            .subscribe(changes => this.updateFilter());

        this.filterProvider.getFilters()
            .first()
            .subscribe(allFilters => {
                for (let filter of allFilters) {
                    if (filter.origin.id == this.graph.id && filter.isOverview) {
                        this.filters.push(filter);
                    }
                }
            });

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
                        }
                    });
            }
        } else {
            this.dimY = null;
        }

        this.updateFilter();
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

    private categoryClick(event: any): void {
        let clickedCategory = this.chart.invert(event.relativePos.y);
        this.chart.setCategoryActive(true, clickedCategory);
    }

    private categoryMoveStart(event: any): void {

    }


    private categoryMoveUpdate(event: any): void {

    }

    private categoryMoveEnd(event: any): void {

    }

    private categoryUpdateFilters(): void {

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
}
