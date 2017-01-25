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
    selectedData: number[] = [];
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

    // indicate lifetime of this component, for subscriptions
    private isActive: boolean = true;

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
        this.isActive = true;
        this.reloadSelection();
        this.loadData(this.graph.dimX, this.graph.dimY);
        this.graph.onDataUpdate
            .takeWhile(() => this.isActive)
            .filter(g => g.changes.indexOf('dimX') > -1 || g.changes.indexOf('dimY') > -1)
            .subscribe(g => this.loadData(g.data.dimX, g.data.dimY));

        this.graphProvider.getGraphs()
            .takeWhile(() => this.isActive)
            .subscribe((graphs) => this.subscribeGraphChanges(graphs));

        this.registerInteractionListeners();
    }

    ngOnDestroy() {
        this.isActive = false;
        this.deregisterInteractionListeners();
    }

    private loadData(dimX: string, dimY: string) {
        if (this.graph.dimX && this.graph.dimY) {
            Observable
                .zip(
                    this.graphDataProvider.getData(this.graph.dimX)
                        .first()
                        // in case dimension changes while loading
                        .takeWhile(() => dimX === this.graph.dimX),
                    this.graphDataProvider.getData(this.graph.dimY)
                        .first()
                        // in case dimension changes while loading
                        .takeWhile(() => dimY === this.graph.dimY))
                .subscribe(([dataX, dataY]) => {
                    this.scatterplot.loadData(dataX, dataY);
                    for (let selection of this.selections) {
                        this.updateSelection(selection);
                    }
                    this.graph.updateData(['selectedDataIndices']);
                    this.highlightData();
                });
        } else {
            this.scatterplot.loadData(null, null);
            this.highlightData();
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
        this.updateSelection(this.currentSelection);
        this.highlightData();
        this.graph.updateData(['selectedDataIndices']);
        
        // don't allow very small polygons
        if (Point.area(this.currentSelection.path) < 200) {
            this.removeSelection(this.currentSelection);
        }
        this.currentSelection = null;
    }

    private handleTouchMove(ev: InteractionEvent): void {
        this.currentSelection.path.push(this.positionInGraph(ev.position));
        this.currentSelection.polygon.paint(this.currentSelection.path);
        let prevSelectionLength = this.currentSelection.selectedData.length;

        if (this.currentSelection.path.length % 10 === 0) {
            this.updateSelection(this.currentSelection);
            this.highlightData();
        }

        let changes = ['selectionPolygons'];
        if (prevSelectionLength !== this.currentSelection.selectedData.length) {
            changes.push('selectedDataIndices');
        }
        this.graph.updateData(changes);
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


    public reloadSelection(): void {
        while (this.selections.length > 0) {
            let sel = this.selections.pop();
            sel.polygon.remove();
        }

        for (let path of this.graph.selectionPolygons) {
            let selection = new Selection();
            selection.path = path;
            selection.polygon = this.scatterplot.createPolygon();
            selection.polygon.paint(selection.path);

            this.selections.push(selection);
        }

        this.highlightData();
    }

    private popupStyle = {
        '-webkit-transform': '',
        '-ms-transform': '',
        'transform': '',
        'visibility': 'hidden'
    }

    private clickedSelection: Selection = null;

    private removeSelection(selection: Selection) {
        selection.polygon.remove();
        _.pull(this.selections, selection);
        _.pull(this.graph.selectionPolygons, selection.path);
        this.graph.updateData(['selectionPolygons', 'selectedDataIndices']);
    }

    private handleClick(ev: InteractionEvent): void {

        if (this.clickedSelection) {
            this.clickedSelection.polygon.setSelected(false);
            this.clickedSelection = null;
            this.popupStyle.visibility = 'hidden';
        } else {
            let pos = this.positionInGraph(ev.position);

            for (let selection of this.selections) {
                let boundingRect = this.buildBoundingRect(selection.path);
                if (pos.isInRectangle(boundingRect)) {
                    this.clickedSelection = selection;
                }
            }

            if (this.clickedSelection !== null) {
                this.clickedSelection.polygon.setSelected(true);
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
            this.clickedSelection.polygon.setSelected(false);
            this.removeSelection(this.clickedSelection);
            this.popupStyle.visibility = 'hidden';
            this.highlightData();
            this.graph.updateData(['selectedDataIndices']);
            this.clickedSelection = null;
        }
    }


    private graphSubscriptions: Subscription[] = []

    private subscribeGraphChanges(graphs: Graph[]) {
        for (let sub of this.graphSubscriptions) {
            sub.unsubscribe();
        }

        this.highlightGraphData(graphs);

        for (let graph of graphs) {
            if (graph !== this.graph) {
                graph.onDataUpdate
                    .takeWhile(() => this.isActive)
                    .filter((u) => u.changes.indexOf('selectedDataIndices') > -1)
                    .subscribe((g) => {
                        this.highlightGraphData(graphs);
                    });
            }
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
        this.graphProvider.getGraphs()
            .first()
            .subscribe((graphs) => {
                this.highlightGraphData(graphs);
            });
    }

    private highlightGraphData(graphs: Graph[]): void {
        let selectionArrays = [];
        for (let selection of this.selections) {
            selectionArrays.push(selection.selectedData);
        }
        let selectedData = _.union.apply(_, selectionArrays);
        this.graph.selectedDataIndices = selectedData;

        let filteredArrays = [];
        for (let graph of graphs) {
            if (graph.selectedDataIndices.length > 0) {
                filteredArrays.push(graph.selectedDataIndices);
            }
        }
        let useFilter = filteredArrays.length > 0;
        let filteredData = _.intersection.apply(_, filteredArrays);

        let values = this.scatterplot.getValues();
        values.highlight(selectedData, filteredData, useFilter);
    }
}
