import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { SocketIO } from '../../services/index';

import { QuatVisualisation } from './quat-visualisation';
import { VecVisualisation } from './vec-visualisation';

@Component({
    selector: 'admin-calibration',
    templateUrl: './app/components/admin-calibration/admin-calibration.html',
    styleUrls: ['./app/components/admin-calibration/admin-calibration.css']
})
export class AdminCalibrationComponent implements OnInit, OnDestroy {

    @ViewChild('rotationOffset')
    private rotationComponent: ElementRef;

    @ViewChild('positionOffset')
    private positionComponent: ElementRef;

    private width: number = window.innerWidth * 0.8;
    private lastUpdateTime: number = 0;

    private socketiofn: any;
    private quatVis: QuatVisualisation;
    private vecVis: VecVisualisation;

    constructor(private socketio: SocketIO) {}

    ngOnInit() {
        this.socketiofn = (data) => this.onSocketData(data);
        this.socketio.on('debug-calibration', this.socketiofn);

        let visWidth = this.width / 2 - 10;
        let visHeight = 500;

        this.quatVis = new QuatVisualisation(this.rotationComponent, visWidth, visHeight);
        this.vecVis = new VecVisualisation(this.positionComponent, visWidth, visHeight);
    }

    ngOnDestroy() {
        this.socketio.off('debug-calibration', this.socketiofn);
        this.quatVis.destroy();
        this.vecVis.destroy();
    }

    private onSocketData(data: any) {
        let packet = JSON.parse(data);
        this.lastUpdateTime = packet.lastUpdateTime;
        this.vecVis.setVector(packet.posOffsetX, packet.posOffsetY, packet.posOffsetZ);
        this.quatVis.setQuat(packet.rotOffsetX, packet.rotOffsetY,
            packet.rotOffsetZ, packet.rotOffsetW);
    }
}

