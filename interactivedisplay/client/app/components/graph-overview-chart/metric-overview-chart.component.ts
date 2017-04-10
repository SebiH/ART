import { Component, Input, AfterViewInit, OnDestroy, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { Graph, Filter, ChartDimension } from '../../models/index';
import { FilterProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

import * as _ from 'lodash';

@Component({
    selector: 'metric-overview-chart',
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

    constructor(private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
    }

    ngOnDestroy() {
    }

    ngOnChanges(changes: SimpleChanges) {

    }
}
