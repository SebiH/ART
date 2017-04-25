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
    private autoContrast: boolean = true;
    private contrastClipping: number = 0;
    private gap: number = 0;
    private gapAutoAdjust: boolean = true;

    private camerasActive: boolean = true;
    private remotePropListener: any;

    constructor(private socketio: SocketIO) {}

    ngOnInit() {
        this.remotePropListener = (jProps) => {
            var props = JSON.parse(jProps);
            this.gain = props.Gain;
            this.exposure = props.Exposure;
            this.blc = props.BLC;
            this.autoContrast = props.AutoContrast;
            this.contrastClipping = props.AutoContrastClipPercent;
            this.gap = props.CameraGap;
            this.gapAutoAdjust = props.GapAutoAdjust;
        };
        this.socketio.on('debug-camera-properties', this.remotePropListener);
    }

    ngOnDestroy() {
        this.socketio.off('debug-camera-properties', this.remotePropListener);
    }

    private setGain(val: number) {
        this.gain = val;
        this.sendCameraProps();
    }

    private setExposure(val: number) {
        this.exposure = val;
        this.sendCameraProps();
    }

    private setBlc(val: number) {
        this.blc = val;
        this.sendCameraProps();
    }

    private toggleAutoContrast() {
        this.autoContrast = !this.autoContrast;
        this.sendCameraProps();
    }

    private setContrastClipping(val: number) {
        this.contrastClipping = val / 100;
        this.sendCameraProps();
    }

    private toggleGapAutoAdjust() {
        this.gapAutoAdjust = !this.gapAutoAdjust;
        this.sendCameraProps();
    }

    private sendCameraProps() {
        this.socketio.sendMessage('camera-properties', {
            gain: this.gain,
            exposure: this.exposure,
            blc: this.blc,
            autoContrast: this.autoContrast,
            autoContrastClipPercent: this.contrastClipping,
            cameraGap: this.gap,
            gapAutoAdjust: this.gapAutoAdjust
        })
    }

    private setGap(val: number) {
        this.gap = val / (100 * 100);
        this.sendCameraProps();
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
