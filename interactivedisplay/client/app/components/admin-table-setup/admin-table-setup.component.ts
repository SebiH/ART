import { Component, OnInit, OnDestroy } from '@angular/core';
import { SocketIO, Settings, SettingsProvider, GraphProvider, DataProvider } from '../../services/index';

@Component({
    selector: 'admin-table-setup',
    templateUrl: './app/components/admin-table-setup/admin-table-setup.html',
    styleUrls: ['./app/components/admin-table-setup/admin-table-setup.css']
})
export class AdminTableSetupComponent implements OnInit, OnDestroy {
    private isActive: boolean = true;

    private corner: number = 0;
    private socketioListener: Function;

    private cornersCalibrated: boolean[] = [false, false, false, false];
    private isCalibrating: boolean = false;
    private calibrationStatus: number = 0;

    private settings: Settings = new Settings();

    private dimensions: string[] = [];

    constructor(
        private socketio: SocketIO,
        private settingsProvider: SettingsProvider,
        private graphProvider: GraphProvider,
        private dataProvider: DataProvider) {
    }

    ngOnInit() {
        this.socketioListener = (update) => this.onStatusUpdate(update);
        this.socketio.on('admin-cmd-calibration-status', this.socketioListener);
        this.settingsProvider.getCurrent()
            .takeWhile(() => this.isActive)
            .subscribe((s) => this.settings = s);
        this.dataProvider.getDimensionNames()
            .first()
            .subscribe((dims) => this.dimensions = dims);
    }

    ngOnDestroy() {
        this.socketio.off('admin-cmd-calibration-status', this.socketioListener);
        this.isActive = false;
    }

    private onStatusUpdate(update: any) {
        let status = JSON.parse(update);
        this.cornersCalibrated[0] = status.topLeftCalibrated;
        this.cornersCalibrated[1] = status.topRightCalibrated;
        this.cornersCalibrated[2] = status.bottomRightCalibrated;
        this.cornersCalibrated[3] = status.bottomLeftCalibrated;
        this.isCalibrating = status.isCalibrating;
        this.calibrationStatus = status.calibrationStatus;
    }

    private calibrateCorner() {
        this.socketio.sendMessage('admin-cmd-set-corner', this.corner);
    }

    private setSurface() {
        this.socketio.sendMessage('admin-cmd-set-surface', null);
    }

    private resetCalibration() {
        this.socketio.sendMessage('admin-cmd-reset-calibration', null);
    }

    private saveSurfaces() {
        this.socketio.sendMessage('admin-cmd-save-surfaces', null);
    }

    private toggleMarkers(): void {
        this.settings.showMarkers = !this.settings.showMarkers;
        this.settingsProvider.sync(this.settings);
    }

    private toggleChart(): void {
        this.settings.showOverviewChart = !this.settings.showOverviewChart;
        this.settingsProvider.sync(this.settings);
    }

    private toggleOverlay(): void {
        this.settings.showMarkerOverlay = !this.settings.showMarkerOverlay;
        this.settingsProvider.sync(this.settings);
    }

    private generateGraphs(baseDim: string): void {
        let posCounter = 0;
        let graphs = this.graphProvider.getGraphs()
            .first()
            .subscribe((graphs) => {
                while (graphs.length > 0) {
                    this.graphProvider.removeGraph(graphs[0]);
                }

                for (let dim of this.dimensions) {
                    if (dim == baseDim) {
                        continue;
                    }

                    let graph = this.graphProvider.addGraph();
                    this.dataProvider.getData(baseDim)
                        .first()
                        .subscribe(data => graph.setDimX(data));
                    this.dataProvider.getData(dim)
                        .first()
                        .subscribe(data => graph.setDimY(data));
                    graph.absolutePos = posCounter;
                    graph.isNewlyCreated = false;
                    posCounter += graph.width;
                }
            });

        setTimeout(() => this.socketio.sendMessage('renew-graphs', null), 1000);
    }

    private lockGraphs(dim: string): void {
        if (this.settings.lockDimension == dim) {
            this.settings.lockDimension = '';
        } else {
            this.settings.lockDimension = dim;
        }
        this.settingsProvider.sync(this.settings);
    }
}
