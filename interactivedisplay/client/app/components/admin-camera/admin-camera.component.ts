import { Component, OnInit, OnDestroy } from '@angular/core';
import { SocketIO } from '../../services/index';

@Component({
    selector: 'admin-camera',
    templateUrl: './app/components/admin-camera/admin-camera.html',
    styleUrls: ['./app/components/admin-camera/admin-camera.css']
})
export class AdminCameraComponent implements OnInit, OnDestroy {

    private gain: number = 1;
    private exposure: number = 8600;
    private blc: number = 0;
    private gap: number = 0;

    private camerasActive: boolean = true;

    constructor(private socketio: SocketIO) {}

    ngOnInit() {
    }

    ngOnDestroy() {

    }

    private setGain(val: number) {
        this.gain = val;
        this.setCamera();
    }

    private setExposure(val: number) {
        this.exposure = val;
        this.setCamera();
    }

    private setBlc(val: number) {
        this.blc = val;
        this.setCamera();
    }

    private setCamera() {
        this.socketio.sendMessage('camera-properties', {
            gain: this.gain,
            exposure: this.exposure,
            blc: this.blc
        })
    }

    private setGap(val: number) {
        this.gap = val / (100 * 100);
        this.socketio.sendMessage('camera-gap', this.gap);
    }

    private setCamerasActive(val: boolean) {
        if (this.camerasActive !== val) {
            this.camerasActive = val;
            this.socketio.sendMessage('camera-active', this.camerasActive);
        }
    }


    private saveCameraSettings(): void {
        this.socketio.sendMessage('save-camera-settings', {});
    }

    private loadCameraSettings(): void {
        this.socketio.sendMessage('load-camera-settings', {});
    }
}
