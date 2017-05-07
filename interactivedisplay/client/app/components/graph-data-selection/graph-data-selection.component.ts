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
  styleUrls: ['./app/components/graph-data-selection/graph-data-selection.css']
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
                this.clickedFilter = null;
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
            if (filter instanceof DetailFilter) {
                let selection = new PathSelection(filter.id, this.chart, filter as DetailFilter);
                this.pathContainer.addPath(selection);
                this.drawFilter(filter, selection);
            }
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
            this.activeFilter = this.createFilter([this.positionInGraph(event.relativePos)]);
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
        if (this.clickedFilter != null && !this.clickedFilter.isSelected) {
            this.clickedFilter = null;
        }

        let prevClickedFilter = this.clickedFilter;

        let pos = this.positionInGraph(event.relativePos);
        let clickedYAxis = event.relativePos.x <= this.margin.left;
        let clickedXAxis = (this.height + this.margin.top) <= event.relativePos.y;
        let dimX = this.graph.isFlipped ? this.graph.dimY : this.graph.dimX;
        let dimY = this.graph.isFlipped ? this.graph.dimX : this.graph.dimY;

        if (dimX == null || dimY == null) {
            return;
        }

        if (clickedXAxis && clickedYAxis) {
            // area is always empty
        } else if (clickedXAxis) {
            if (dimX.isMetric) {
                this.toggleMetricFilter(pos, 'x');
            } else {
                this.toggleCategoryFilter(pos, 'x');
            }

        } else if (clickedYAxis) {
            if (dimY.isMetric) {
                this.toggleMetricFilter(pos, 'y');
            } else {
                this.toggleCategoryFilter(pos, 'y');
            }


        } else if (this.clickedFilter && this.clickedFilter.isSelected) {
            this.filterProvider.setSelected(null);
            this.clickedFilter = null;
        }

        let point = new Point(pos[0], pos[1]);

        for (let filter of this.getActiveFilters()) {
            let boundingRect = null;
            if (filter instanceof DetailFilter) {
                let df = filter as DetailFilter;
                boundingRect = [new Point(df.minX, df.minY), new Point(df.maxX, df.maxY)];
            } else {
                boundingRect = Utils.buildBoundingRect(filter.path);
            }

            if (point.isInRectangle(boundingRect) && filter != prevClickedFilter) {
                this.clickedFilter = filter;
                this.filterProvider.setSelected(filter);
                let transform = 'translate3d(' + event.relativePos.x + 'px,' + event.relativePos.y + 'px,0)';
                this.popupStyle['-webkit-transform'] = transform;
                this.popupStyle['-ms-transform'] = transform;
                this.popupStyle['transform'] = transform;
                break;
            }
        }
    }

    private toggleMetricFilter(position: [number, number], axis: 'x' | 'y') {
        let dimX = this.graph.isFlipped ? this.graph.dimY : this.graph.dimX;
        let dimY = this.graph.isFlipped ? this.graph.dimX : this.graph.dimY;

        let pos: number;
        if (this.graph.isFlipped) {
            pos = axis == 'x' ? position[1] : position[0];
        } else {
            pos = axis == 'x' ? position[0] : position[1];
        }

        let path: [number, number][] = null;

        for (let bin of (axis == 'x' ? dimX : dimY).bins) {
            if (bin.range) {
                if (bin.range[0] <= pos && pos <= bin.range[1]) {
                    if (axis == 'x') {
                        path = [
                            [bin.range[0], dimY.getMinValue()],
                            [bin.range[0], dimY.getMaxValue()],
                            [bin.range[1], dimY.getMaxValue()],
                            [bin.range[1], dimY.getMinValue()],
                        ];
                    } else {
                        path = [
                            [dimX.getMinValue(), bin.range[0]],
                            [dimX.getMaxValue(), bin.range[0]],
                            [dimX.getMaxValue(), bin.range[1]],
                            [dimX.getMinValue(), bin.range[1]],
                        ];
                    }
                    break;
                }
            } else {
                if (bin.value == Math.floor(pos)) {
                    if (axis == 'x') {
                        path = [
                            [bin.value, dimY.getMinValue()],
                            [bin.value, dimY.getMaxValue()],
                            [bin.value + 0.9999, dimY.getMaxValue()],
                            [bin.value + 0.9999, dimY.getMinValue()],
                        ];
                    } else {
                        path = [
                            [dimX.getMinValue(), bin.value],
                            [dimX.getMaxValue(), bin.value],
                            [dimX.getMaxValue(), bin.value + 0.9999],
                            [dimX.getMinValue(), bin.value + 0.9999],
                        ];
                    }
                    break;
                }
            }
        }

        if (this.graph.isFlipped) {
            for (let i = 0; i < path.length; i++) {
                path[i] = [path[i][1], path[i][0]];
            }
        }

        if (path != null) {
            let hasSamePath = this.removeFilterWithSamePath(path);

            if (!hasSamePath) {
                let filter = this.createFilter(path);
                filter.useAxisColor = axis;
            }
        }

    }


    private toggleCategoryFilter(position: [number, number], axis: 'x' | 'y') {
        let dimX = this.graph.isFlipped ? this.graph.dimY : this.graph.dimX;
        let dimY = this.graph.isFlipped ? this.graph.dimX : this.graph.dimY;
        let pos: number;
        if (this.graph.isFlipped) {
            pos = axis == 'x' ? position[1] : position[0];
        } else {
            pos = axis == 'x' ? position[0] : position[1];
        }


        for (let map of (axis == 'x' ? dimX : dimY).mappings) {
            if (map.value - 0.5 <= pos && pos <= map.value + 0.5) {
                let path: [number, number][] = [
                    [map.value - 0.5, dimY.getMinValue()],
                    [map.value - 0.5, dimY.getMaxValue()],
                    [map.value + 0.5, dimY.getMaxValue()],
                    [map.value + 0.5, dimY.getMinValue()]
                ];

                if (axis == 'y') {
                    path = [
                        [dimX.getMinValue(), map.value - 0.5],
                        [dimX.getMaxValue(), map.value - 0.5],
                        [dimX.getMaxValue(), map.value + 0.5],
                        [dimX.getMinValue(), map.value + 0.5]
                    ]
                }


                if (this.graph.isFlipped) {
                    for (let i = 0; i < path.length; i++) {
                        path[i] = [path[i][1], path[i][0]];
                    }
                }

                let hasSamePath = this.removeFilterWithSamePath(path);

                if (!hasSamePath) {
                    let filter = this.createFilter(path);
                    filter.useAxisColor = 'n';
                    filter.color = map.color;
                }
                break;
            }
        }
    }

    private removeFilterWithSamePath(path: [number, number][]): boolean {
        for (let filter of this.getActiveFilters()) {
            let hasSamePath = false;
            if (filter.path.length == path.length) {
                hasSamePath = true;
                for (let i = 0; i < filter.path.length; i++) {
                    hasSamePath = hasSamePath && filter.path[i][0] == path[i][0] && filter.path[i][1] == path[i][1];
                }
            }

            if (hasSamePath) {
                this.filterProvider.removeFilter(filter);
                return true;
            }
        }

        return false;
    }


    private createFilter(points: [number, number][]): DetailFilter {

        let filter = this.filterProvider.createDetailFilter(this.graph);
        filter.isUserGenerated = true;
        filter.boundDimensions = 'xy';

        for (let point of points) {
            filter.addPathPoint(point);
        }

        this.drawFilter(filter, this.getSelection(filter));

        this.filterProvider.setSelected(null);
        this.clickedFilter = null;

        return filter;
    }

    /*
     *    Data highlights
     */
    private drawHighlights(): void {

    }
}
