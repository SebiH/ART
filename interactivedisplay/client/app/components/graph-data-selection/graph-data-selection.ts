import { Component, AfterViewInit, OnDestroy, Input, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { Chart2dComponent } from '../chart-2d/chart-2d.component';
import { PathSelection } from '../chart-2d/path-selection';
import { PathContainer } from '../chart-2d/path-container';
import { Graph, Filter, FilterType, FilterPoint, ChartDimension, Point } from '../../models/index';
import { GraphProvider, FilterProvider, DataHighlight } from '../../services/index';
import { Utils } from '../../Utils';

import * as _ from 'lodash';

interface Selection {
    filter: Filter;
    path: PathSelection
}

@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements AfterViewInit, OnDestroy, OnChanges {

    @Input() graph: Graph;
    @Input() dimX: ChartDimension;
    @Input() dimY: ChartDimension;

    @Input() public width = 600;
    @Input() public height = 600;
    @Input() public margin = { top: 50, right: 50, bottom: 100, left: 100 };

    @ViewChild(Chart2dComponent) private chart: Chart2dComponent;

    // indicate lifetime of this component, for subscriptions
    private isActive: boolean = true;

    private selections: Selection[] = [];
    private currentSelection: Selection;
    private pathContainer: PathContainer;
    private graphs: Graph[] = [];

    constructor(private filterProvider: FilterProvider, private graphProvider: GraphProvider) {}

    ngAfterViewInit() {
        this.graphProvider.getGraphs()
            .takeWhile(() => this.isActive)
            .subscribe((graphs) => this.graphs = graphs);

        this.pathContainer = new PathContainer();
        this.chart.addElement(this.pathContainer);

        this.filterProvider.getFilters()
            .takeWhile(() => this.isActive)
            .subscribe((filters) => this.initFilters(filters));

        let currentFilter: DataHighlight[] = null;
        this.filterProvider.globalFilterUpdate()
            .takeWhile(() => this.isActive)
            .subscribe(filter => {
                currentFilter = filter;
                this.highlightData(filter);
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .subscribe(filter => {
                if (currentFilter != null) {
                    this.highlightData(currentFilter);
                }
            });
    }

    ngOnDestroy() {
        this.isActive = false;
    }


    ngOnChanges(changes: SimpleChanges) {
        if (this.pathContainer && (changes['dimX'] || changes['dimY'])) {
            this.filterProvider.getFilters()
                .first()
                .subscribe((filters) => {
                    this.initFilters(filters);
                });
        }
    }


    private initFilters(filters: Filter[]): void {
        for (let selection of this.selections) {
            this.pathContainer.removePath(selection.path);
        }
        this.selections = [];

        for (let filter of filters) {
            if (filter.origin.id === this.graph.id) {
                let selection = {
                    filter: filter,
                    path: new PathSelection(this.chart, filter.color)
                };
                this.pathContainer.addPath(selection.path);
                this.selections.push(selection);
            }
        }

        this.updatePaths();
    }

    private updatePaths(): void {
        for (let selection of this.selections) {
            this.updatePath(selection);
        }
    }

    private updatePath(selection: Selection): void {
        selection.path.setPath(selection.filter.path);
    }


    private handleTouchDown(event): void {
        if (!(this.graph.dimX) || !(this.graph.dimY)) {
            return;
        }

        let filter = this.filterProvider.createFilter(this.graph);
        filter.category = FilterType.Detail;

        let path = new PathSelection(this.chart, filter.color);
        this.pathContainer.addPath(path);

        this.currentSelection = {
            filter: filter,
            path: path
        };

        filter.path.push(this.positionInGraph(event.relativePos));
        this.updatePath(this.currentSelection);
        this.selections.push(this.currentSelection);
    }

    private handleTouchUp(event): void {
        if (!(this.graph.dimX) || !(this.graph.dimY)) {
            return;
        }

        if (Point.areaOf(this.currentSelection.filter.path) < Math.max(this.dimX.domain.max, this.dimY.domain.max) / 100) {
            // avoid small polygons
            this.removeSelection(this.currentSelection);
        } else { 
            this.updatePath(this.currentSelection);
            this.filterProvider.updateFilter(this.currentSelection.filter);

            if (this.currentSelection.filter.indices.length == 0){
                this.removeSelection(this.currentSelection);
            }
        }

        this.currentSelection = null;
    }

    private handleTouchMove(event): void {
        if (!(this.graph.dimX) || !(this.graph.dimY)) {
            return;
        }

        this.currentSelection.filter.path.push(this.positionInGraph(event.relativePos));

        // TODO: disabled because selection lags behind?
        // try to reduce points by detecting straight lines
        // let length = this.currentSelection.filter.path.length;
        // if (length > 3)
        // {
        //     let lineStart = this.currentSelection.filter.path[length - 3];
        //     let lineEnd = this.currentSelection.filter.path[length - 1];
        //     let point = this.currentSelection.filter.path[length - 2];

        //     if (Point.isOnLine(point, lineStart, lineEnd)) {
        //         this.currentSelection.filter.path.splice(length - 2, 1);
        //     }
        // }

        this.updatePath(this.currentSelection);

        if (this.currentSelection.filter.path.length % 10 === 0) {
            this.filterProvider.updateFilter(this.currentSelection.filter);
        }
    }

    private positionInGraph(p: Point): [number, number] {
        return this.chart.invert([p.x - this.margin.left, p.y - this.margin.top]);
    }

    private highlightData(globalFilter: DataHighlight[]): void {
        let attributes = [];

        let totalGraphCount = this.graphs.length;

        for (let gfData of globalFilter) {
            let stroke = gfData.color == '#FFFFFF' ? '#000000' : gfData.color;
            let fill = '#000000';
            let radius = 10;

            if (gfData.selectedBy.indexOf(this.graph.id) < 0 && gfData.selectedBy.length == this.graphs.length - 1) {
                fill = '#616161';
                radius = 7;
            } else if (gfData.selectedBy.length < this.graphs.length) {
                fill = 'transparent';
                radius = 1;
            }

            attributes.push({
                stroke: stroke,
                fill: fill,
                radius: radius
            });

        }

        this.chart.setAttributes(attributes);
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
        this.filterProvider.removeFilter(selection.filter);
        this.pathContainer.removePath(selection.path);
        _.pull(this.selections, selection);
    }

    private handleClick(event): void {

        if (this.clickedSelection) {
            this.clickedSelection.path.setSelected(false);
            this.clickedSelection = null;
            this.popupStyle.visibility = 'hidden';
        } else {
            let pos = this.positionInGraph(event.relativePos);
            let point = new Point(pos[0], pos[1]);

            for (let selection of this.selections) {
                let boundingRect = Utils.buildBoundingRect(selection.filter.path);
                if (point.isInRectangle(boundingRect)) {
                    this.clickedSelection = selection;
                }
            }

            if (this.clickedSelection !== null) {
                this.clickedSelection.path.setSelected(true);
                let transform = 'translate3d(' + (event.relativePos.x - 100) + 'px,' + (event.relativePos.y - 25) + 'px,0)';
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
            this.clickedSelection.path.setSelected(false);
            this.removeSelection(this.clickedSelection);
            this.popupStyle.visibility = 'hidden';
            this.clickedSelection = null;
        }
    }
}
