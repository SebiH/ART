import { Component, OnInit, OnDestroy, Input, ViewChild, ElementRef } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { Graph, Point } from '../../models/index';
import { ScatterPlotComponent, ChartPolygon } from '../scatter-plot/scatter-plot';
import {
    GraphProvider,
    GraphDataProvider,
    InteractionManager,
    InteractionEvent,
    InteractionListener,
    InteractionEventType
} from '../../services/index';

@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements OnInit, OnDestroy {

    @Input()
    private graph: Graph;

    @ViewChild('plot')
    private scatterplot: ScatterPlotComponent;
    @ViewChild('plotContainer')
    private graphContainer: ElementRef;

    private graphSubscription: Subscription;

    private data: Point[];

    private prevDimX: string;
    private prevDimY: string;

    private currentSelection: Point[];
    private currentPolygon: ChartPolygon;
    private selectionPolygons: ChartPolygon[] = [];

    private clickListener: InteractionListener;
    private touchDownListener: InteractionListener;
    private touchMoveListener: InteractionListener;
    private touchUpListener: InteractionListener;

    constructor(
        private graphProvider: GraphProvider,
        private graphDataProvider: GraphDataProvider,
        private interactionManager: InteractionManager) {}

    ngOnInit() {
        this.loadData();
        this.loadExistingSelection();
        this.graphSubscription = this.graph.onDataUpdate
            .subscribe(() => {
                this.loadData();
            });

        this.registerInteractionListeners();
    }

    ngOnDestroy() {
        this.graphSubscription.unsubscribe();
        this.deregisterInteractionListeners();
    }


    private loadData() {
        let hasBothDimensions = this.graph.dimX && this.graph.dimY;
        let hasNewDimX = this.prevDimX !== this.graph.dimX;
        let hasNewDimY = this.prevDimY !== this.graph.dimY;
        let hasNewDimension = hasNewDimX || hasNewDimY;

        if (hasBothDimensions && hasNewDimension) {
            this.prevDimX = this.graph.dimX;
            this.prevDimY = this.graph.dimY;
            Observable
                .zip(
                    this.graphDataProvider.getData(this.graph.dimX).first(),
                    this.graphDataProvider.getData(this.graph.dimY).first())
                .subscribe(([dataX, dataY]) => {
                    this.scatterplot.loadData(dataX, dataY);
                    this.highlightData();
                });
        }
    }




    private registerInteractionListeners(): void {
        this.clickListener = {
            type: InteractionEventType.Click,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleClick(ev); }
        };

        this.touchDownListener = {
            type: InteractionEventType.TouchDown,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleTouchDown(ev); }
        };
        this.touchMoveListener = {
            type: InteractionEventType.TouchMove,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleTouchMove(ev); }
        };
        this.touchUpListener = {
            type: InteractionEventType.TouchUp,
            element: this.graphContainer.nativeElement,
            handler: (ev) => { this.handleTouchUp(ev); }
        };
        this.interactionManager.on(this.clickListener);
        this.interactionManager.on(this.touchDownListener);
        this.interactionManager.on(this.touchMoveListener);
        this.interactionManager.on(this.touchUpListener);
    }


    private deregisterInteractionListeners(): void {
        this.interactionManager.off(this.clickListener);
        this.interactionManager.off(this.touchDownListener);
        this.interactionManager.off(this.touchMoveListener);
        this.interactionManager.off(this.touchUpListener);
    }

    private handleTouchDown(ev: InteractionEvent): void {
        this.currentSelection = [];
        this.graph.selectionPolygon.push(this.currentSelection);

        this.currentPolygon = this.scatterplot.createSelectionPolygon();
        this.selectionPolygons.push(this.currentPolygon);
    }

    private handleTouchUp(ev: InteractionEvent): void {
        this.highlightData();
    }

    private handleTouchMove(ev: InteractionEvent): void {
        let globalPosition = this.graphContainer.nativeElement.getBoundingClientRect();
        let posOffset = new Point(
            globalPosition.left + this.scatterplot.margin.left,
            globalPosition.top + this.scatterplot.margin.top); 

        this.currentSelection.push(Point.sub(ev.position, posOffset));
        this.currentPolygon.paint(this.currentSelection);
        this.highlightData();
    }



    private loadExistingSelection(): void {
        for (let path of this.graph.selectionPolygon) {
            let polygon = this.scatterplot.createSelectionPolygon();
            polygon.paint(path);
            this.selectionPolygons.push(polygon);
        }
    }

    private handleClick(ev: InteractionEvent): void {
        // TODO.
    }


    private clearSelection(): void {
        while (this.graph.selectionPolygon.length > 0) {
            this.graph.selectionPolygon.pop();
        }

        for (let sel of this.selectionPolygons) {
            sel.remove();
        }
        this.selectionPolygons = [];

        this.highlightData();
    }

    private highlightData(): void {
    //     if (this.data) {
    //         while (this.graph.selectedDataIndices.length > 0) {
    //             this.graph.selectedDataIndices.pop();
    //         }

    //         if (this.graph.selectionPolygon.length > 0) {
    //             let topLeft = new Point(this.graph.selectionPolygon[0].x, this.graph.selectionPolygon[0].y);
    //             let bottomRight = new Point(this.graph.selectionPolygon[0].x, this.graph.selectionPolygon[0].y);

    //             for (let p of this.graph.selectionPolygon) {
    //                 topLeft.x = Math.min(topLeft.x, p.x);
    //                 topLeft.y = Math.min(topLeft.y, p.y);
    //                 bottomRight.x = Math.max(bottomRight.x, p.x);
    //                 bottomRight.y = Math.max(bottomRight.y, p.y);
    //             }

    //             for (let index = 0; index < this.data.length; index++) {
    //                 let datum = new Point(this.scaleX(this.data[index].x), this.scaleY(this.data[index].y));
    //                 if (datum.isInPolygon(this.graph.selectionPolygon, [topLeft, bottomRight])) {
    //                     this.graph.selectedDataIndices.push(index);
    //                 }
    //             }

    //         }

    //         this.graph.updateData();

    //         // highlight data
    //         d3.select(this.graphContainer.nativeElement)
    //             .selectAll('circle')
    //             .filter((d, i) => this.graph.selectedDataIndices.indexOf(i) > -1)
    //             .style('fill', 'red');

    //         // remove highlight from other data
    //         d3.select(this.graphContainer.nativeElement)
    //             .selectAll('circle')
    //             .filter((d, i) => this.graph.selectedDataIndices.indexOf(i) == -1)
    //             .style('fill', 'black');
    //     }
    }
}
