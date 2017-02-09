import { Component, Input } from '@angular/core';
import { Graph } from '../../models/index';

@Component({
    selector: 'graph-list-item',
    templateUrl: './app/components/graph-list-item/graph-list-item.html',
    styleUrls: ['./app/components/graph-list-item/graph-list-item.css'],
})
export class GraphListItemComponent {
    @Input()
    private graph: Graph;

    @Input()
    private offset: number;

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
            width: this.graph.width
        };

        return style;
    }
}
