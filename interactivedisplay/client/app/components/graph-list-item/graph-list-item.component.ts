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
        let transform = 'translate3d(' + this.graph.posOffset + 'px, 0, 0)';
        let style = {
            '-webkit-transform': transform,
            '-ms-transform': transform,
            transform: transform
        };

        return style;
    }

    private getListItemStyle() {
        let transform = 'translate3d(' + this.offset + 'px, 0, 0)';
        let style = {
            '-webkit-transform': transform,
            '-ms-transform': transform,
            transform: transform,
            width: this.graph.width,
            'z-index': this.graph.listIndex + (this.graph.isPickedUp ? 100 : 0)
        };

        return style;
    }
}
