import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Graph } from '../../models/index';

@Component({
  selector: 'graph-section',
  templateUrl: './app/components/graph-section/graph-section.html',
  styleUrls: ['./app/components/graph-section/graph-section.css'],
})
export class GraphSectionComponent implements OnInit, OnDestroy {

    @Input()
    private graph: Graph;

    constructor () {}

    ngOnInit() {
    }

    ngOnDestroy() {

    }
}
