import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Marker, Graph } from '../../models/index';

@Component({
    selector: 'graph-overview',
    templateUrl: './app/components/graph-overview/graph-overview.html',
    styleUrls: ['./app/components/graph-overview/graph-overview.css'],
})
export class GraphOverviewComponent implements OnInit, OnDestroy {

    @Input()
    private graph: Graph;

    constructor() {}

    ngOnInit() {

    }

    ngOnDestroy() {

    }
}
