import { Component, OnInit, OnDestroy, Input, EventEmitter, Output } from '@angular/core';
import {} from '../../services/index';
import { Graph } from '../../models/index';

@Component({
  selector: 'graph-detail',
  templateUrl: './app/components/graph-detail/graph-detail.html',
  styleUrls: ['./app/components/graph-detail/graph-detail.css'],
})
export class GraphDetailComponent implements OnInit, OnDestroy {

    @Input() graph: Graph;

    @Output() onClose = new EventEmitter();

    constructor () {}

    ngOnInit() {
    }

    ngOnDestroy() {
    }

    private close(): void {
        this.onClose.emit();
    }
}
