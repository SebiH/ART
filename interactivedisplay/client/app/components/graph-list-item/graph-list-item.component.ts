import { Component, Input, OnChanges } from '@angular/core';
import { Graph } from '../../models/index';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'graph-list-item',
    templateUrl: './app/components/graph-list-item/graph-list-item.html',
    styleUrls: ['./app/components/graph-list-item/graph-list-item.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphListItemComponent implements OnChanges {
    @Input() private graph: Graph;
    @Input() private offset: number;

    constructor(private changeDetector: ChangeDetectorRef) {
    }

    ngOnChanges() {
        this.changeDetector.markForCheck();
    }

    private getGraphStyle() {
        let style = {
            left: this.graph.posOffset + 'px'
        };

        return style;
    }

    private getListItemStyle() {
        let style = {
            left: this.offset + 'px',
            width: this.graph.width,
            'z-index': this.graph.listIndex + (this.graph.isPickedUp ? 100 : 0)
        };

        return style;
    }
}
