import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Graph } from '../../models/index';
import { GraphProvider, Settings, SettingsProvider, DataProvider, Dimension } from '../../services/index';

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
    private phases: string[] = [];

    constructor(
        private graphProvider: GraphProvider,
        private dataProvider: DataProvider,
        private settingsProvider: SettingsProvider,
        private changeDetector: ChangeDetectorRef) {
    }

    ngOnInit() {
        this.dataProvider.getPhases()
            .first()
            .subscribe(phases => this.phases = phases);

        this.graph.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isColored') >= 0)
            .subscribe(() => this.changeDetector.markForCheck());

        this.settingsProvider.getCurrent()
            .takeWhile(() => this.isActive)
            .subscribe((s) => {
                this.settings = s;
                this.changeDetector.markForCheck();
            });
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

    private setPhase(phase: string) {
        this.dataProvider.getDimensions()
            .first()
            .subscribe(dims => {
                if (this.graph.getActualXAxis()) {
                    let foundMatch = false;
                    let prevDim = this.graph.getActualXAxis();
                    for (let dim of dims) {
                        if (dim.phases.indexOf(phase) >= 0 && dim.displayName == prevDim.displayName) {
                            foundMatch = true;
                            this.dataProvider.getData(dim.name)
                                .first()
                                .subscribe(data => this.graph.setDimX(data));
                            break;
                        }
                    }

                    if (!foundMatch) {
                        this.graph.setDimX(null);
                    }
                }

                if (this.graph.getActualYAxis()) {
                    let foundMatch = false;
                    let prevDim = this.graph.getActualYAxis();
                    for (let dim of dims) {
                        if (dim.phases.indexOf(phase) >= 0 && dim.displayName == prevDim.displayName) {
                            foundMatch = true;
                            this.dataProvider.getData(dim.name)
                                .first()
                                .subscribe(data => this.graph.setDimY(data));
                            break;
                        }
                    }

                    if (!foundMatch) {
                        this.graph.setDimY(null);
                    }
                }

                this.graph.phase = phase;
            });



    }
}
