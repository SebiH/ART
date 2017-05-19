import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Graph } from '../../models/index';
import { GraphProvider, Settings, SettingsProvider } from '../../services/index';

@Component({
    selector: 'graph-detail',
    templateUrl: './app/components/graph-detail/graph-detail.html',
    styleUrls: ['./app/components/graph-detail/graph-detail.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphDetailComponent implements OnInit, OnDestroy {
    @Input() graph: Graph;
    private isActive: boolean = true;
    private settings: Settings;

    constructor(
        private graphProvider: GraphProvider,
        private settingsProvider: SettingsProvider,
        private changeDetector: ChangeDetectorRef) {
    }

    ngOnInit() {
        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isColored') >= 0)
            .subscribe(() => this.changeDetector.markForCheck());

        this.settingsProvider.getCurrent()
            .takeWhile(() => this.isActive)
            .subscribe((s) => this.settings = s);
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private toggleColor() {
        if (this.graph.isColored) {
            this.graphProvider.setColor(null);
        } else {
            this.graphProvider.setColor(this.graph);
        }
    }
}
