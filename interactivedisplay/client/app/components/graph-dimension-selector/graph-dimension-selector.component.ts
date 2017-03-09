import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { GraphDataProvider } from '../../services/index';
import { Graph, ChartDimension } from '../../models/index'; 

const BORDER_SIZE = 30;

@Component({
    selector: 'graph-dimension-selector',
    templateUrl: './app/components/graph-dimension-selector/graph-dimension-selector.html',
    styleUrls: [ './app/components/graph-dimension-selector/graph-dimension-selector.css' ]
})
export class GraphDimensionSelectorComponent implements OnInit {
    @Input() axis: 'x' | 'y';
    @Input() graph: Graph;
    @Input() size: number = 1000;

    private dimensions: string[];
    private offset: number = 0;
    private listStyle = {
        '-webkit-transform': '',
        transform: ''
    };

    private scrollerStyle: any = {};

    constructor(private graphDataProvider: GraphDataProvider) {}

    ngOnInit() {
        this.graphDataProvider.getDimensions()
            .first()
            .subscribe((dims) => this.dimensions = dims);

        if (this.axis === 'x') {
            this.scrollerStyle = {
                'max-width': this.size - BORDER_SIZE * 2,
                'margin': '0 ' + BORDER_SIZE + 'px'
            };
        } else {
            this.scrollerStyle = {
                'max-height': this.size - BORDER_SIZE * 2,
                'margin': BORDER_SIZE + 'px 0'
            };
        }
    }

    private setDimension(dim: string): void {
        if (this.axis === 'x') {
            this.graph.dimX = dim;
        } else {
            this.graph.dimY = dim;
        }
    }

    private updateOffset(): void {
        // calculate max offset
        let transform = '';
        if (this.axis === 'x') {
            transform = 'translate3d(' + this.offset + 'px, 0, 0)';
        } else {
            transform = 'translate3d(0, ' + this.offset + 'px, 0)';
        }

        this.listStyle['-webkit-transform'] = transform;
        this.listStyle.transform = transform;
    }



    private onDragBegin(ev: any): void {

    }

    private onDragUpdate(ev: any): void {
        if (this.axis === 'x') {
            this.offset -= ev.deltaX;
        } else {
            this.offset -= ev.deltaY;
        }

        this.updateOffset();
    }

    private onDragEnd(ev: any): void {

    }
}

