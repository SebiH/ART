import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Graph, ChartDimension } from '../../models/index';
import { GraphDataProvider } from '../../services/index';
import { Chart1dComponent } from '../chart-1d/chart-1d.component';

@Component({
    selector: 'graph-overview-chart',
    template: `<chart-1d [height]="height"
                         [width]="width"
                         [dimension]="graph.isFlipped ? dimY : dimX"

                         touch-button
                         (touchclick)="onClick($event)"

                         moveable
                         (moveStart)="onMoveStart($event)"
                         (moveUpdate)="onMoveUpdate($event)"
                         (moveEnd)="onMoveEnd($event)">
               </chart-1d>`
})
export class GraphOverviewChartComponent implements OnInit, OnDestroy {
    @Input() graph: Graph;
    @Input() width: number;
    @Input() height: number;

    @ViewChild(Chart1dComponent) chart: Chart1dComponent;

    private isActive: boolean = true;
    private dimX: ChartDimension = null;
    private dimY: ChartDimension = null;

    constructor(private dataProvider: GraphDataProvider) {}

    ngOnInit() {
        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('dimX') >= 0 || changes.indexOf('dimY') >= 0)
            .subscribe(changes => this.updateDimensions(this.graph.dimX, this.graph.dimY));

        this.updateDimensions(this.graph.dimX, this.graph.dimY);
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private updateDimensions(newDimX: string, newDimY: string): void {
        if (newDimX) {
            if (this.dimX === null || this.dimX.name !== newDimX) {
                this.dataProvider.getData(newDimX)
                    .first()
                    .subscribe(data => {
                        // just in case it has changed in the meantime
                        if (newDimX === this.graph.dimX) {
                            this.dimX = data;
                        }
                    });
            }
        } else {
            this.dimX = null;
        }

        if (newDimY) {
            if (this.dimY === null || this.dimY.name !== newDimY) {
                this.dataProvider.getData(newDimY)
                    .first()
                    .subscribe(data => {
                        // just in case it has changed in the meantime
                        if (newDimY === this.graph.dimY) {
                            this.dimY = data;
                        }
                    });
            }
        } else {
            this.dimY = null;
        }
    }


    private onClick(event: any) {
        console.log(this.chart.invert(event.relativePos.y));
    }


    private onMoveStart(event: any) {

    }

    private onMoveUpdate($event: any) {

    }

    private onMoveEnd($event: any) {

    }

}
