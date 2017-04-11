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
export class GraphOverviewChartComponent {
    @Input() graph: Graph;
    @Input() width: number;
    @Input() height: number;
    @Input() dim: ChartDimension = null;

    constructor() {}
}
