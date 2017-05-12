import { Component, OnInit, OnDestroy } from '@angular/core';
import { SocketIO, Settings, SettingsProvider } from '../../services/index';

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

    constructor(private socketio: SocketIO, private settingsProvider: SettingsProvider) {
    }

    ngOnInit() {
        this.socketioListener = (update) => this.onStatusUpdate(update);
        this.socketio.on('admin-cmd-calibration-status', this.socketioListener);
        this.settingsProvider.getCurrent()
            .takeWhile(() => this.isActive)
            .subscribe((s) => this.settings = s);
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
}
