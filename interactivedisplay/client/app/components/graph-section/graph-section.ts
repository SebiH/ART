import { Component, Input, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Marker, Graph } from '../../models/index';
import { MarkerProvider, GraphProvider, Settings, SettingsProvider } from '../../services/index';

const NUM_MARKERS = 8;

@Component({
    selector: 'graph-section',
    templateUrl: './app/components/graph-section/graph-section.html',
    styleUrls: ['./app/components/graph-section/graph-section.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphSectionComponent implements OnInit, OnDestroy {

    @Input() private graph: Graph;

    private markers: Marker[] = [];
    private isActive: boolean = true;
    private showOverview: boolean = false;
    private showDetail: boolean = false;
    private isAnyGraphSelected: boolean = false;

    private settings: Settings = new Settings();

    constructor (
        private markerProvider: MarkerProvider,
        private graphProvider: GraphProvider,
        private elementRef: ElementRef,
        private settingsProvider: SettingsProvider,
        private changeDetector: ChangeDetectorRef
        ) {}

    ngOnInit() {
        for (let i = 0; i < NUM_MARKERS; i++) {
            this.markers.push(this.markerProvider.createMarker());
        }

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .subscribe(() => {
                this.changeDetector.markForCheck();
            });

        this.graphProvider.onGraphSelectionChanged()
            .takeWhile(() => this.isActive)
            .subscribe(selectedGraph => {
                this.isAnyGraphSelected = (selectedGraph != null);
                this.changeDetector.markForCheck();
            });

        this.settingsProvider.getCurrent()
            .takeWhile(() => this.isActive)
            .subscribe(s => {
                this.settings = s;
                this.changeDetector.markForCheck();
            });


        // quick hack to not show either detail nor overview graph while graph
        // is in transition from being (de)selected
        // (especially since angular animations do not seem to work in Edge)
        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isSelected') >= 0)
            .subscribe(() => {
                this.showDetail = false;
                this.showOverview = false;
                this.changeDetector.markForCheck();
            });

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isSelected') >= 0)
            .delay(600)
            .subscribe(() => this.setIsSelected());
        this.setIsSelected();
    }

    ngOnDestroy() {
        for (let marker of this.markers) {
            this.markerProvider.destroyMarker(marker);
        }

        this.isActive = false;
    }

    private setIsSelected() {
        if (this.graph.isSelected) {
            this.showOverview = false;
            this.showDetail = true;
        } else {
            this.showOverview = true;
            this.showDetail = false;
        }

        this.changeDetector.markForCheck();
    }

    public getSectionPosition() {
        let element = <HTMLElement>this.elementRef.nativeElement;
        return element.getBoundingClientRect();
    }

    private toggleColor() {
        if (this.graph.isColored) {
            this.graphProvider.setColor(null);
        } else {
            this.graphProvider.setColor(this.graph);
        }

        this.changeDetector.markForCheck();
    }

    private toggleFlip() {
        this.graph.isFlipped = !this.graph.isFlipped;
        this.changeDetector.markForCheck();
    }

    private selectGraph(): void {
        if (this.showOverview) {
            this.graphProvider.selectGraph(this.graph);
            this.changeDetector.markForCheck();
        }
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
        if (this.showDetail) {
            this.graphProvider.selectGraph(null);
        }
    }

    private toggleSort(): void {
        this.graph.sortAxis = !this.graph.sortAxis;
    }

}
