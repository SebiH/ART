import { Component, Input, AfterViewInit, OnDestroy } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { DataProvider, Dimension } from '../../services/index';
import { Graph, ChartDimension } from '../../models/index';

import * as _ from 'lodash';

const BORDER_SIZE = 30;
const VERTICAL_ITEM_SIZE = 240;
const HORIZONTAL_ITEM_SIZE = 350;

@Component({
    selector: 'graph-dimension-selector',
    templateUrl: './app/components/graph-dimension-selector/graph-dimension-selector.html',
    styleUrls: [ './app/components/graph-dimension-selector/graph-dimension-selector.css' ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphDimensionSelectorComponent implements AfterViewInit, OnDestroy {
    @Input() axis: 'x' | 'y';
    @Input() graph: Graph;
    @Input() size: number = 1000;

    private dimensions: Dimension[] = [];
    private activeDimensions: Dimension[] = [];
    private offset: number = 0;
    private maxOffset: number = 0;
    private hasTouchDown: boolean = false;
    private isActive: boolean = true;

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

    constructor(private dataProvider: DataProvider, private changeDetector: ChangeDetectorRef) {}

    ngAfterViewInit() {
        this.dataProvider.getDimensions()
            .first()
            .subscribe((dims) => {
                this.dimensions = dims;
                this.init();
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('phase') >= 0)
            .subscribe(() => {
                this.init();
                this.offset = 0;
                this.updateOffset();
                this.changeDetector.markForCheck();
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isFlipped') >= 0)
            .subscribe(() => this.scrollToCurrent());

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

        this.changeDetector.markForCheck();
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private init() {
        this.activeDimensions = _.filter(this.dimensions, { phase: this.graph.phase });
        let itemSize = this.getItemSize();

        if (this.axis === 'x') {
            this.maxOffset = this.activeDimensions.length * itemSize - this.size / 2 + BORDER_SIZE * 2;
        } else {
            this.maxOffset = this.activeDimensions.length * itemSize - this.size / 2 + BORDER_SIZE * 2;
        }

        this.scrollToCurrent();
        this.changeDetector.markForCheck();
        // needs to update twice for some reason
        setTimeout(() => this.changeDetector.markForCheck());
    }

    private scrollToCurrent(): void {
        let graphDim = this.getActiveDim();
        let dim = graphDim ? graphDim.name : '';
        let itemSize = this.getItemSize();
        if (graphDim && this.activeDimensions) {
            this.offset = _.findIndex(this.activeDimensions, { name: dim }) * itemSize - this.size / 2 + BORDER_SIZE * 2;
            this.updateOffset();
        }
    }

    private setDimension(dim: string): void {
        let prevDim = this.getActiveDim();

        if (this.graph.isFlipped) {
            if (this.axis === 'x') {
                this.applyDimension(dim, 'y');
            } else {
                this.applyDimension(dim, 'x');
            }
        } else {
            if (this.axis === 'x') {
                this.applyDimension(dim, 'x');
            } else {
                this.applyDimension(dim, 'y');
            }
        }

        this.changeDetector.markForCheck();
    }

    private applyDimension(dim: string, axis: 'x' | 'y') {
        this.dataProvider.getData(dim)
            .first()
            .subscribe((data) => {
                if (axis == 'x') {
                    this.graph.dimX = data;
                } else {
                    this.graph.dimY = data;
                }

                this.changeDetector.markForCheck();
            })
    }

    private getActiveDim(): ChartDimension {
        return this.axis === 'x' ? this.graph.getCurrentXAxis() : this.graph.getCurrentYAxis();
    }

    private getItemSize(): number {
        return (this.axis == 'x') ? HORIZONTAL_ITEM_SIZE : VERTICAL_ITEM_SIZE;
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

        this.changeDetector.markForCheck();
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
            .takeWhile(() => this.hasBeforeTouchDown && this.offset >= 0 && this.isActive)
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
            .takeWhile(() => this.hasAfterTouchDown && this.offset < this.maxOffset && this.isActive)
            .subscribe(() => {
                this.offset += itemSize / 3;
                this.updateOffset();
            });
    }

    private onAfterTouchUp(): void {
        this.hasAfterTouchDown = false;
    }

}
