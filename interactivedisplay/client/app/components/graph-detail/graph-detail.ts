import { Component, Input } from '@angular/core';
import { Graph, ChartDimension } from '../../models/index';

@Component({
    selector: 'graph-detail',
    templateUrl: './app/components/graph-detail/graph-detail.html',
    styleUrls: ['./app/components/graph-detail/graph-detail.css'],
})
export class GraphDetailComponent {
    @Input() graph: Graph;
    @Input() dimX: ChartDimension;
    @Input() dimY: ChartDimension;
}
