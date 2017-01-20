import { Component, AfterViewInit, OnDestroy, Input, ViewChild, ElementRef } from '@angular/core';
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

import * as _ from 'lodash';

class Selection {
    path: Point[] = [];
    polygon: ChartPolygon;
    selectedData: number[] = null;
}


@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements AfterViewInit, OnDestroy {

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

    private selections: Selection[] = [];
    private currentSelection: Selection;

    private clickListener: InteractionListener;
    private touchDownListener: InteractionListener;
    private touchMoveListener: InteractionListener;
    private touchUpListener: InteractionListener;

    constructor(
        private graphProvider: GraphProvider,
        private graphDataProvider: GraphDataProvider,
        private interactionManager: InteractionManager) {}

    ngAfterViewInit() {
        this.loadData();
        this.loadExistingSelection();
        this.graphSubscription = this.graph.onDataUpdate
            .subscribe(() => this.loadData());

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
                    for (let selection of this.selections) {
                        this.updateSelection(selection);
                    }
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
        this.currentSelection = new Selection();
        this.graph.selectionPolygons.push(this.currentSelection.path);

        this.currentSelection.polygon = this.scatterplot.createPolygon();
        this.selections.push(this.currentSelection);
    }

    private handleTouchUp(ev: InteractionEvent): void {
        this.graph.updateData();
    }

    private handleTouchMove(ev: InteractionEvent): void {
        this.currentSelection.path.push(this.positionInGraph(ev.position));
        this.currentSelection.polygon.paint(this.currentSelection.path);
        this.updateSelection(this.currentSelection);
        this.highlightData();
    }


    private positionInGraph(p: Point): Point {
        let globalPosition = this.graphContainer.nativeElement.getBoundingClientRect();
        let posOffset = new Point(
            globalPosition.left + this.scatterplot.margin.left,
            globalPosition.top + this.scatterplot.margin.top); 

        return Point.sub(p, posOffset);
    }

    private buildBoundingRect(polygon: Point[]): Point[] {
        if (polygon.length === 0) {
            return [new Point(0, 0), new Point(0, 0)];
        }

        let topLeft = new Point(polygon[0].x, polygon[0].y);
        let bottomRight = new Point(polygon[0].x, polygon[0].y);

        for (let p of polygon) {
            topLeft.x = Math.min(topLeft.x, p.x);
            topLeft.y = Math.min(topLeft.y, p.y);
            bottomRight.x = Math.max(bottomRight.x, p.x);
            bottomRight.y = Math.max(bottomRight.y, p.y);
        }

        return [topLeft, bottomRight];
    }


    private loadExistingSelection(): void {
        for (let path of this.graph.selectionPolygons) {
            let selection = new Selection();
            selection.path = path;
            selection.polygon = this.scatterplot.createPolygon();
            selection.polygon.paint(selection.path);

            this.selections.push(selection);
        }
    }

    private popupStyle = {
        '-webkit-transform': '',
        '-ms-transform': '',
        'transform': '',
        'visibility': 'hidden'
    }

    private clickedSelection: Selection = null;

    private handleClick(ev: InteractionEvent): void {

        if (this.clickedSelection) {
            this.clickedSelection = null;
            this.popupStyle.visibility = 'hidden';
        } else {
            let pos = this.positionInGraph(ev.position);

            for (let selection of this.selections) {
                let boundingRect = this.buildBoundingRect(selection.path);
                if (pos.isInPolygon(selection.path, boundingRect)) {
                    this.clickedSelection = selection;
                }
            }

            if (this.clickedSelection !== null) {
                let transform = 'translate3d(' + pos.x + 'px,' + pos.y + 'px,0)';
                this.popupStyle.visibility = 'visible';
                this.popupStyle['-webkit-transform'] = transform;
                this.popupStyle['-ms-transform'] = transform;
                this.popupStyle['transform'] = transform;
            } else {
                this.popupStyle.visibility = 'hidden';
            }
        }
    }

    private popupClick() {
        if (this.clickedSelection) {
            this.clickedSelection.polygon.remove();
            _.pull(this.selections, this.clickedSelection);
            _.pull(this.graph.selectionPolygons, this.clickedSelection.path);
            this.popupStyle.visibility = 'hidden';
            this.highlightData();
            this.clickedSelection = null;
        }
    }


    private updateSelection(selection: Selection): void {
        let data = this.scatterplot.data;
        let boundingRect = this.buildBoundingRect(selection.path);
        selection.selectedData = [];

        for (let i = 0; i < data.length; i++) {
            let p = new Point(data[i][0], data[i][1]);
            if (p.isInPolygon(selection.path, boundingRect)) {
                selection.selectedData.push(i);
            }
        }
    }

    private highlightData(): void {
        let selectionArrays = [];
        for (let selection of this.selections) {
            selectionArrays.push(selection.selectedData);
        }

        let selectedData = _.union.apply(_, selectionArrays);
        let values = this.scatterplot.getValues();
        values.highlight(selectedData);
    }

    private selectData(polygon: Point[]): number[] {
        return [];
    }
}
