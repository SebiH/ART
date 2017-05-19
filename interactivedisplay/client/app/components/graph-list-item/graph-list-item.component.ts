import { Component, Input, OnChanges, ViewChild } from '@angular/core';
import { Graph } from '../../models/index';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { GraphSectionComponent } from '../graph-section/graph-section';

@Component({
    selector: 'graph-list-item',
    templateUrl: './app/components/graph-list-item/graph-list-item.html',
    styleUrls: ['./app/components/graph-list-item/graph-list-item.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphListItemComponent implements OnChanges {
    @ViewChild(GraphSectionComponent) graphSection: GraphSectionComponent;

    @Input() graph: Graph;
    @Input() offset: number;

    constructor(private changeDetector: ChangeDetectorRef) {
    }

    ngOnChanges() {
        this.changeDetector.markForCheck();
    }

    public getPosition() {
        return this.graphSection.getSectionPosition();
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
