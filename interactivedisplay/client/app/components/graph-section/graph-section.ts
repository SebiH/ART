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

    constructor (
        private markerProvider: MarkerProvider,
        private graphProvider: GraphProvider,
        private elementRef: ElementRef
        ) {}

    ngOnInit() {
        for (let i = 0; i < 2; i++) {
            this.markers.push(this.markerProvider.createMarker());
        }

        Observable.timer(0, 50)
            .takeWhile(() => this.isActive)
            .subscribe(this.checkForChanges.bind(this));

        this.graph.onDataUpdate
            .takeWhile(() => this.isActive)
            .filter(g => g.changes.indexOf('isSelected') > -1)
            .subscribe(g => {
                if (this.graph.isSelected) {
                    this.graph.width *= 2;
                } else {
                    this.graph.width /= 2;
                }
                this.graph.updatePosition();
            });
    }

    ngOnDestroy() {
        for (let marker of this.markers) {
            this.markerProvider.destroyMarker(marker);
        }

        this.isActive = false;
    }

    private checkForChanges(): void {
        let currentPosition = this.getSectionPosition().x;

        if (this.graph.absolutePos == undefined || this.graph.absolutePos !== currentPosition) {
            this.graph.absolutePos = currentPosition;
            this.graph.updatePosition();
        }
    }

    private getSectionPosition(): Point {
        let element = <HTMLElement>this.elementRef.nativeElement;
        let pos = element.getBoundingClientRect();

        return new Point(pos.left, pos.top);
    }


    private selectGraph(): void {
        if (!this.graph.isSelected) {
            this.graph.isSelected = true;
            this.graph.updateData(['isSelected']);
        }
    }

    private deselectGraph(): void {
        if (this.graph.isSelected) {
            this.graph.isSelected = false;
            this.graph.updateData(['isSelected']);
        }
    }

    private deleteGraph(): void {
        this.graphProvider.removeGraph(this.graph);
    }

    private handleMoveStart(graph: Graph, event: any) {
        this.onMove.emit({
            start: true,
        });
    }

    private handleMoveUpdate(graph: Graph, event: any) {
        this.onMove.emit({
            delta: event.deltaX
        });
    }

    private handleMoveEnd(graph: Graph, event: any) {
        this.onMove.emit({
            end: true,
        });
    }

}
