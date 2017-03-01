import { Component, Input, OnInit, OnDestroy, ElementRef, Output, EventEmitter } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { Marker, Graph, Point } from '../../models/index';
import { MarkerProvider, GraphProvider } from '../../services/index';

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

    constructor (
        private markerProvider: MarkerProvider,
        private graphProvider: GraphProvider,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        for (let i = 0; i < 7; i++) {
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

}
