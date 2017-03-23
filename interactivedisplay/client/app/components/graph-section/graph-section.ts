import { Component, Input, OnInit, OnDestroy, ElementRef, animate, trigger, state, transition, style } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { Marker, Graph, Point, ChartDimension } from '../../models/index';
import { MarkerProvider, GraphProvider, GraphDataProvider, FilterProvider } from '../../services/index';

const NUM_MARKERS = 8;

@Component({
    selector: 'graph-section',
    templateUrl: './app/components/graph-section/graph-section.html',
    styleUrls: ['./app/components/graph-section/graph-section.css'],
    animations: [
        trigger('visibility', [
            state('visible', style({
                opacity: 1,
                visibility: 'visible'
            })),
            state('hidden', style({
                opacity: 0,
                visibility: 'collapse'
            })),
            transition('* => *', [ animate('0.5s 500ms linear') ])
    ])
    ]
})
export class GraphSectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;

    private markers: Marker[] = [];
    private isActive: boolean = true;
    private isAnyGraphSelected: boolean = false;

    private dimX: ChartDimension = null;
    private dimY: ChartDimension = null;

    constructor (
        private markerProvider: MarkerProvider,
        private graphProvider: GraphProvider,
        private dataProvider: GraphDataProvider,
        private filterProvider: FilterProvider,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        for (let i = 0; i < NUM_MARKERS; i++) {
            this.markers.push(this.markerProvider.createMarker());
        }

        Observable.timer(0, 50)
            .takeWhile(() => this.isActive)
            .subscribe(this.checkForChanges.bind(this));

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('dimX') >= 0 || changes.indexOf('dimY') >= 0)
            .subscribe(changes => this.updateDimensions(this.graph.dimX, this.graph.dimY));

        this.graphProvider.onGraphSelectionChanged()
            .takeWhile(() => this.isActive)
            .subscribe(selectedGraph => {
                this.isAnyGraphSelected = (selectedGraph != null);
            });

        this.updateDimensions(this.graph.dimX, this.graph.dimY);
    }

    ngOnDestroy() {
        for (let marker of this.markers) {
            this.markerProvider.destroyMarker(marker);
        }

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


    private checkForChanges(): void {
        let pos = this.getSectionPosition();
        let currentPosition = pos.left + pos.width / 2;

        if (this.graph.absolutePos == undefined || this.graph.absolutePos !== currentPosition) {
            this.graph.absolutePos = currentPosition;
        }
    }

    private getSectionPosition() {
        let element = <HTMLElement>this.elementRef.nativeElement;
        return element.getBoundingClientRect();
    }

    private toggleColor() {
        if (this.graph.isColored) {
            this.graphProvider.setColor(null);
        } else {
            this.graphProvider.setColor(this.graph);
        }

        this.filterProvider.triggerGlobalFilterUpdate();
    }

    private toggleFlip() {
        this.filterProvider.removeFilters(this.graph);
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

        if (this.isAnyGraphSelected) {
            this.graphProvider.selectGraph(null);
        }
    }

    private handleMoveUpdate(event: any) {
        this.graph.posOffset -= event.deltaX;
    }

    private handleMoveEnd(event: any) {
        this.graph.isPickedUp = false;
        this.graph.posOffset = 0;
    }

    private closeSelection(): void {
        this.graphProvider.selectGraph(null);
    }

}
