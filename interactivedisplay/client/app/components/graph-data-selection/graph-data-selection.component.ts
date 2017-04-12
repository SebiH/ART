import { Component, AfterViewInit, OnDestroy, Input, ViewChild } from '@angular/core';
import { Chart2dComponent } from '../chart-2d/chart-2d.component';
import { PathSelection } from '../chart-2d/path-selection';
import { PathContainer } from '../chart-2d/path-container';
import { Graph, Filter, DetailFilter, Point } from '../../models/index';
import { FilterProvider, GlobalFilterProvider } from '../../services/index';
import { Utils } from '../../Utils';

import * as _ from 'lodash';


@Component({
  selector: 'graph-data-selection',
  templateUrl: './app/components/graph-data-selection/graph-data-selection.html',
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css'],
})
export class GraphDataSelectionComponent implements AfterViewInit, OnDestroy {
    @Input() graph: Graph;

    @Input() public width = 600;
    @Input() public height = 600;
    @Input() public margin = { top: 50, right: 50, bottom: 200, left: 200 };

    @ViewChild(Chart2dComponent) private chart: Chart2dComponent;

    private isActive: boolean = true;
    private filters: Filter[] = [];
    private pathContainer: PathContainer;

    constructor(
        private filterProvider: FilterProvider,
        private globalFilterProvider: GlobalFilterProvider) {
    }

    ngAfterViewInit() {
        this.pathContainer = new PathContainer();
        this.chart.addElement(this.pathContainer);

        this.filterProvider.getFilters()
            .takeWhile(() => this.isActive)
            .subscribe((filters) => {
                this.filters = filters;
                this.drawFilters();
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .subscribe(() => {
                // let chart component update first
                setTimeout(() => {
                    this.drawFilters();
                    this.drawHighlights();
                });
            });
    }

    ngOnDestroy() {
        this.isActive = false;
    }


    /*
     *    Util
     */

    private getActiveFilters(): Filter[] {
        return _.filter(this.filters, f => f.origin.id == this.graph.id);
    }

    private getSelection(filter: Filter): PathSelection {
        return _.find(this.pathContainer.getPaths(), p => p.id == filter.id);
    }

    private positionInGraph(p: Point): [number, number] {
        let pos = this.chart.invert([p.x - this.margin.left, p.y - this.margin.top]);

        if (this.graph.isFlipped) {
            return [pos[1], pos[0]];
        } else {
            return pos;
        }
    }


    /*
     *    Draw filters
     */
    private drawFilters(): void {
        this.pathContainer.clear();

        for (let filter of this.getActiveFilters()) {
            let selection = new PathSelection(filter.id, this.chart, filter.getColor());
            this.pathContainer.addPath(selection);
            this.drawFilter(filter, selection);
        }
    }

    private drawFilter(filter: Filter, selection: PathSelection) {
        if (this.graph.isFlipped) {
            selection.setPath(this.flip(filter.path));
        } else {
            selection.setPath(filter.path);
        }
    }

    private flip(path: [number, number][]): [number, number][] {
        let flippedPath: [number, number][] = [];
        for (let p of path) {
            flippedPath.push([p[1], p[0]]);
        }
        return flippedPath;
    }



    /*
     *    Filter creation
     */
    private activeFilter: DetailFilter = null;

    private handleTouchDown(event): void {
        if (this.graph.dimX !== null && this.graph.dimY !== null) {
            this.activeFilter = this.filterProvider.createDetailFilter(this.graph);
            this.activeFilter.isUserGenerated = true;
            this.activeFilter.boundDimensions = 'xy';
            this.activeFilter.addPathPoint(this.positionInGraph(event.relativePos));
            this.drawFilter(this.activeFilter, this.getSelection(this.activeFilter));
        }
    }


    private handleTouchUp(event): void {
        if (this.activeFilter) {
            let filter = this.activeFilter;
            this.activeFilter.onUpdate
                .filter(changes => changes.indexOf('selectedDataIndices') >= 0)
                .first()
                .subscribe(() => {
                    if (filter.selectedDataIndices.length == 0) {
                        setTimeout(() => this.filterProvider.removeFilter(filter));
                    }
                });

            this.activeFilter.addPathPoint(this.positionInGraph(event.relativePos));
            this.drawFilter(this.activeFilter, this.getSelection(this.activeFilter));
            this.activeFilter = null;
        }
    }

    private handleTouchMove(event): void {
        if (this.activeFilter) {
            this.activeFilter.addPathPoint(this.positionInGraph(event.relativePos));
            this.drawFilter(this.activeFilter, this.getSelection(this.activeFilter));
        }
    }



    /*
     *    Filter removal
     */

    private popupStyle = {
        '-webkit-transform': '',
        '-ms-transform': '',
        'transform': ''
    }

    private clickedFilter: Filter = null;

    private handleClick(event): void {
        if (this.clickedFilter) {
            this.getSelection(this.clickedFilter).setSelected(false);
            this.clickedFilter = null;
        } else {

            let pos = this.positionInGraph(event.relativePos);
            let point = new Point(pos[0], pos[1]);

            for (let filter of this.getActiveFilters()) {
                let boundingRect = Utils.buildBoundingRect(filter.path);
                if (point.isInRectangle(boundingRect)) {
                    this.clickedFilter = filter;
                    this.getSelection(filter).setSelected(true);
                    let transform = 'translate3d(' + (event.relativePos.x - 100) + 'px,' + (event.relativePos.y - 25) + 'px,0)';
                    this.popupStyle['-webkit-transform'] = transform;
                    this.popupStyle['-ms-transform'] = transform;
                    this.popupStyle['transform'] = transform;
                    break;
                }
            }
        }
    }


    private popupClick() {
        if (this.clickedFilter) {
            this.filterProvider.removeFilter(this.clickedFilter);
            this.clickedFilter = null;
        }
    }


    /*
     *    Data highlights
     */
    private drawHighlights(): void {

    }
}
