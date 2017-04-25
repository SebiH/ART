import { Component, Input } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Graph } from '../../models/index';
import { GraphProvider } from '../../services/index';

@Component({
    selector: 'graph-detail',
    templateUrl: './app/components/graph-detail/graph-detail.html',
    styleUrls: ['./app/components/graph-detail/graph-detail.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphDetailComponent {
    @Input() graph: Graph;

    constructor(private graphProvider: GraphProvider) {

    }

    private useColorX() {
        this.graphProvider.setColor2(this.graph, 'x');
    }

    private useColorY() {
        this.graphProvider.setColor2(this.graph, 'y');
    }
}
