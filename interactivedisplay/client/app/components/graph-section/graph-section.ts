import { Component, Input, OnInit, OnDestroy, ElementRef, Output, EventEmitter } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { Marker, Graph, Point, ChartDimension } from '../../models/index';
import { MarkerProvider, GraphProvider, GraphDataProvider } from '../../services/index';

const NUM_MARKERS = 6;

@Component({
    selector: 'graph-section',
    templateUrl: './app/components/graph-section/graph-section.html',
    styleUrls: ['./app/components/graph-section/graph-section.css'],
})
export class GraphSectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;

    @Output() onMove = new EventEmitter(); 

    private markers: Marker[] = [];
    private isActive: boolean = true;
    private isAnyGraphSelected: boolean = false;

    private dimX: ChartDimension = null;
    private dimY: ChartDimension = null;

    constructor (
        private markerProvider: MarkerProvider,
        private graphProvider: GraphProvider,
        private dataProvider: GraphDataProvider,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        for (let i = 0; i < NUM_MARKERS; i++) {
            this.markers.push(this.markerProvider.createMarker());
        }

        Observable.timer(0, 50)
            .takeWhile(() => this.isActive)
            .subscribe(this.checkForChanges.bind(this));

        this.graphProvider.onGraphSelectionChanged()
            .takeWhile(() => this.isActive)
            .subscribe(selectedGraph => {
                this.isAnyGraphSelected = (selectedGraph != null);
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('dimX') >= 0 || changes.indexOf('dimY') >= 0)
            .subscribe(changes => this.updateDimensions(this.graph.dimX, this.graph.dimY));

        this.updateDimensions(this.graph.dimX, this.graph.dimY);
    }

    ngOnDestroy() {
        for (let marker of this.markers) {
            this.markerProvider.destroyMarker(marker);
        }

        this.isActive = false;
    }

    private checkForChanges(): void {
        let currentPosition = this.getSectionPosition().x + this.graph.width / 2;

        if (this.graph.absolutePos == undefined || this.graph.absolutePos !== currentPosition) {
            this.graph.absolutePos = currentPosition;
        }
    }

    private getSectionPosition(): Point {
        let element = <HTMLElement>this.elementRef.nativeElement;
        let pos = element.getBoundingClientRect();

        return new Point(pos.left, pos.top);
    }

    private toggleColor() {
        // TODO1 should be done in graphProvider, probably
        this.graph.isColored = !this.graph.isColored;
    }

    private toggleFlip() {
        this.graph.isFlipped = !this.graph.isFlipped;
    }

    private selectGraph(): void {
        this.graphProvider.selectGraph(this.graph);
    }

    private deleteGraph(): void {
        this.graphProvider.removeGraph(this.graph);
    }

    private handleMoveStart(event: any) {
        this.graph.isPickedUp = true;
    }

    private handleMoveUpdate(event: any) {
        this.graph.posOffset -= event.deltaX;
    }

    private handleMoveEnd(event: any) {
        this.graph.isPickedUp = false;
        this.graph.posOffset = 0;
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
}
