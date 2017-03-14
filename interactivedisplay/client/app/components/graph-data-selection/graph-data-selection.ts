import { Component, AfterViewInit, OnDestroy, Input, ViewChild } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { Chart2dComponent } from '../chart-2d/chart-2d.component';
import { PathSelection } from '../chart-2d/path-selection';
import { Graph, Filter, ChartDimension, Point } from '../../models/index';
import { FilterProvider } from '../../services/index';

import * as _ from 'lodash';

interface Selection {
    filter: Filter;
    polygon: PathSelection
}

@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements AfterViewInit, OnDestroy {

    @Input() graph: Graph;
    @Input() dimX: ChartDimension;
    @Input() dimY: ChartDimension;

    @Input() public width = 600;
    @Input() public height = 600;
    @Input() public margin = { top: 50, right: 50, bottom: 100, left: 100 };

    @ViewChild('plot') private scatterplot: Chart2dComponent;

    // indicate lifetime of this component, for subscriptions
    private isActive: boolean = true;

    private selections: Selection[] = [];
    private currentSelection: Selection;

    constructor(private filterProvider: FilterProvider) {}

    ngAfterViewInit() {
        this.isActive = true;
        this.reloadSelection();
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private handleTouchDown(event): void {
        // this.currentSelection = new Selection();
        // // TODO1
        // // this.graph.selectionPolygons.push(this.currentSelection.path);

        // this.currentSelection.polygon = this.scatterplot.createPolygon();
        // this.selections.push(this.currentSelection);
    }

    private handleTouchUp(event): void {
        // if (Point.areaOf(this.currentSelection.path) < 200) {
        //     // avoid small polygons
        //     this.removeSelection(this.currentSelection);
        // } else { 
        //     this.calculateSelectedData(this.currentSelection);
        //     this.updateSelectedGraphData();
        // }

        // this.currentSelection = null;
    }

    private handleTouchMove(event): void {
        // let pos = this.positionInGraph(ev.position);
        // this.currentSelection.path.push([pos.x, pos.y]);
        // this.currentSelection.polygon.paint(this.currentSelection.path);

        // let length = this.currentSelection.path.length;
        // // try to reduce points by detecting straight lines
        // if (length > 2)
        // {
        //     let lineStart = this.currentSelection.path[length - 3];
        //     let lineEnd = this.currentSelection.path[length - 1];
        //     let point = this.currentSelection.path[length - 2];

        //     if (Point.isOnLine(point, lineStart, lineEnd)) {
        //         this.currentSelection.path.splice(length - 2, 1);
        //     }
        // }

        // if (this.currentSelection.path.length % 10 === 0) {
        //     this.calculateSelectedData(this.currentSelection);
        // }
    }


    // private positionInGraph(p: Point): Point {
        // let globalPosition = this.graphContainer.nativeElement.getBoundingClientRect();
        // let posOffset = new Point(
        //     globalPosition.left + this.scatterplot.margin.left,
        //     globalPosition.top + this.scatterplot.margin.top); 

        // return Point.sub(p, posOffset);
    // }

    private buildBoundingRect(polygon: number[][]): Point[] {
        if (polygon.length === 0) {
            return [new Point(0, 0), new Point(0, 0)];
        }

        let topLeft = new Point(polygon[0][0], polygon[0][1]);
        let bottomRight = new Point(polygon[0][1], polygon[0][1]);

        for (let p of polygon) {
            topLeft.x = Math.min(topLeft.x, p[0]);
            topLeft.y = Math.min(topLeft.y, p[1]);
            bottomRight.x = Math.max(bottomRight.x, p[0]);
            bottomRight.y = Math.max(bottomRight.y, p[1]);
        }

        return [topLeft, bottomRight];
    }


    public reloadSelection(): void {
        // while (this.selections.length > 0) {
        //     let sel = this.selections.pop();
        //     sel.polygon.remove();
        // }

        // TODO1
        // for (let path of this.graph.selectionPolygons) {
        //     let selection = new Selection();
        //     selection.path = path;
        //     selection.polygon = this.scatterplot.createPolygon();
        //     selection.polygon.paint(selection.path);

        //     this.selections.push(selection);
        // }
    }

    private calculateSelectedData(selection: Selection): void {
        // let data = this.scatterplot.data;
        // let boundingRect = this.buildBoundingRect(selection.path);
        // selection.selectedData = [];

        // for (let i = 0; i < data.length; i++) {
        //     let p = new Point(data[i][0], data[i][1]);
        //     if (p.isInPolygonOf(selection.path, boundingRect)) {
        //         selection.selectedData.push(i);
        //     }
        // }

        // this.updateSelectedGraphData();
    }

    private updateSelectedGraphData(): void {
        // let selectionArrays = [];
        // for (let selection of this.selections) {
        //     selectionArrays.push(selection.selectedData);
        // }
        // let selectedData = _.union.apply(_, selectionArrays);
        // // TODO1
        // // this.graph.selectedDataIndices = selectedData;
        // // this.graph.updateData(['selectedDataIndices']);
    }

    private highlightData(globalFilter: number[]): void {
        // let values = this.scatterplot.getValues();
        // TODO1
        // values.highlight(this.graph.selectedDataIndices, globalFilter);
    }



    /*
     * selection removal
     */

    private popupStyle = {
        '-webkit-transform': '',
        '-ms-transform': '',
        'transform': '',
        'visibility': 'hidden'
    }

    private clickedSelection: Selection = null;

    private removeSelection(selection: Selection) {
        // selection.polygon.remove();
        // _.pull(this.selections, selection);
        // // TODO1
        // // _.pull(this.graph.selectionPolygons, selection.path);
        // this.updateSelectedGraphData();
    }

    private handleClick(event): void {

        // if (this.clickedSelection) {
        //     this.clickedSelection.polygon.setSelected(false);
        //     this.clickedSelection = null;
        //     this.popupStyle.visibility = 'hidden';
        // } else {
        //     let pos = this.positionInGraph(ev.position);

        //     for (let selection of this.selections) {
        //         let boundingRect = this.buildBoundingRect(selection.path);
        //         if (pos.isInRectangle(boundingRect)) {
        //             this.clickedSelection = selection;
        //         }
        //     }

        //     if (this.clickedSelection !== null) {
        //         this.clickedSelection.polygon.setSelected(true);
        //         let transform = 'translate3d(' + pos.x + 'px,' + (pos.y + 25) + 'px,0)';
        //         this.popupStyle.visibility = 'visible';
        //         this.popupStyle['-webkit-transform'] = transform;
        //         this.popupStyle['-ms-transform'] = transform;
        //         this.popupStyle['transform'] = transform;
        //     } else {
        //         this.popupStyle.visibility = 'hidden';
        //     }
        // }
    }

    private popupClick() {
        // if (this.clickedSelection) {
        //     this.clickedSelection.polygon.setSelected(false);
        //     this.removeSelection(this.clickedSelection);
        //     this.popupStyle.visibility = 'hidden';
        //     this.clickedSelection = null;
        // }
    }
}
