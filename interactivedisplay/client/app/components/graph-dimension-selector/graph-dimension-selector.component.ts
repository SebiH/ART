import { Component, Input, AfterViewInit, OnDestroy } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { GraphDataProvider, FilterProvider } from '../../services/index';
import { Graph, ChartDimension, Filter } from '../../models/index'; 

const BORDER_SIZE = 30;
const ITEM_WIDTH = 200;
const ITEM_HEIGHT = 140;

@Component({
    selector: 'graph-dimension-selector',
    templateUrl: './app/components/graph-dimension-selector/graph-dimension-selector.html',
    styleUrls: [ './app/components/graph-dimension-selector/graph-dimension-selector.css' ]
})
export class GraphDimensionSelectorComponent implements AfterViewInit {
    @Input() axis: 'x' | 'y';
    @Input() graph: Graph;
    @Input() size: number = 1000;

    private dimensions: string[];
    private offset: number = 0;
    private maxOffset: number = 0;
    private hasTouchDown: boolean = false;

    private scrollerStyle: any = {};
    private listStyle = {
        '-webkit-transform': '',
        transform: '',
        transition: ''
    };

    private triangleBeforeStyle = {
        'opacity': 1,
    };
    private triangleAfterStyle = {
        'opacity': 1
    };

    constructor(private graphDataProvider: GraphDataProvider, private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.graphDataProvider.getDimensions()
            .first()
            .subscribe((dims) => {
                this.dimensions = dims;

                if (this.axis === 'x') {
                    this.maxOffset = dims.length * ITEM_WIDTH - this.size / 2 + BORDER_SIZE * 2;
                } else {
                    this.maxOffset = dims.length * ITEM_HEIGHT - this.size / 2 + BORDER_SIZE * 2;
                }

                let graphDim = (this.axis == 'x') ? this.graph.dimX : this.graph.dimY;
                let itemSize = (this.axis == 'x') ? ITEM_WIDTH : ITEM_HEIGHT;
                if (graphDim) {
                    this.offset = dims.indexOf(graphDim) * itemSize - this.size / 2 + BORDER_SIZE * 2;
                    this.updateOffset();
                }
            });

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
        let prevDim = "";
        if (this.axis === 'x') {
            prevDim = this.graph.dimX;
            this.graph.dimX = dim;
        } else {
            prevDim = this.graph.dimY;
            this.graph.dimY = dim;
        }

        if (this.graph.dimY !== prevDim || this.graph.dimX !== prevDim) {
            this.filterProvider.removeFilters(this.graph);
        }
    }

    private getItemSize(): number {
        return (this.axis == 'x') ? ITEM_WIDTH : ITEM_HEIGHT;
    }

    private updateOffset(): void {
        this.offset = Math.max(-this.size / 2, Math.min(this.offset, this.maxOffset));

        // calculate max offset
        let transform = '';
        if (this.axis === 'x') {
            transform = 'translate3d(' + (-this.offset) + 'px, 0, 0)';
        } else {
            transform = 'translate3d(0, ' + (-this.offset) + 'px, 0)';
        }

        let transition = '';
        if (!this.hasTouchDown) {
            transition = 'transform 0.5s ease-out';
        }

        this.listStyle['-webkit-transform'] = transform;
        this.listStyle.transform = transform;
        this.listStyle.transition = transition;


        let itemSize = this.getItemSize();
        if (this.offset <= itemSize / 2) {
            this.triangleBeforeStyle.opacity = 0;
        } else {
            this.triangleBeforeStyle.opacity = 1;
        }

        if (this.offset >= this.maxOffset - this.size / 2 - itemSize / 2) {
            this.triangleAfterStyle.opacity = 0;
        } else {
            this.triangleAfterStyle.opacity = 1;
        }
    }



    private onDragBegin(ev: any): void {
        this.hasTouchDown = true;
        this.updateOffset();
    }

    private onDragUpdate(ev: any): void {
        if (this.axis === 'x') {
            this.offset += ev.deltaX;
        } else {
            this.offset += ev.deltaY;
        }

        this.updateOffset();
    }

    private onDragEnd(ev: any): void {
        this.hasTouchDown = false;
        this.updateOffset();
    }



    private scrollBack(): void {
        let itemSize = this.getItemSize();
        this.offset -= itemSize;
        this.updateOffset();
    }

    private scrollForward(): void {
        let itemSize = this.getItemSize();
        this.offset += itemSize;
        this.updateOffset();
    }



    private hasBeforeTouchDown: boolean = false;

    private onBeforeTouchDown(): void {
        let itemSize = this.getItemSize();
        this.hasBeforeTouchDown = true;
        Observable.timer(0, 100)
            .takeWhile(() => this.hasBeforeTouchDown && this.offset >= 0)
            .subscribe(() => {
                this.offset -= itemSize / 3;
                this.updateOffset();
            });
    }

    private onBeforeTouchUp(): void {
        this.hasBeforeTouchDown = false;
    }





    private hasAfterTouchDown: boolean = false;

    private onAfterTouchDown(): void {
        let itemSize = this.getItemSize();
        this.hasAfterTouchDown = true;
        Observable.timer(0, 100)
            .takeWhile(() => this.hasAfterTouchDown && this.offset < this.maxOffset)
            .subscribe(() => {
                this.offset += itemSize / 3;
                this.updateOffset();
            });
    }

    private onAfterTouchUp(): void {
        this.hasAfterTouchDown = false;
    }

}

