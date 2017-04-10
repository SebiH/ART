import { Component, Input } from '@angular/core';
import { Graph } from '../../models/index';

@Component({
    selector: 'graph-detail',
    templateUrl: './app/components/graph-detail/graph-detail.html',
    styleUrls: ['./app/components/graph-detail/graph-detail.css'],
})
export class GraphDetailComponent {
    @Input() graph: Graph;
}
