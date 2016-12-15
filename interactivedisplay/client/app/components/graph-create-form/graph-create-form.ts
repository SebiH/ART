import { Component, OnInit, OnDestroy } from '@angular/core';
import { GraphProvider } from '../../services/index';
import { Graph } from '../../models/index';

@Component({
  selector: 'graph-create-form',
  templateUrl: './app/components/graph-create-form/graph-create-form.html',
  styleUrls: ['./app/components/graph-create-form/graph-create-form.css'],
})
export class GraphCreateFormComponent implements OnInit, OnDestroy {

    constructor (private graphProvider: GraphProvider) {}

    ngOnInit() {
    }

    ngOnDestroy() {
    }
}
