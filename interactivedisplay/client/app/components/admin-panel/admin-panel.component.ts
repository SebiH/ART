import { Component } from '@angular/core';
import { SocketIO } from '../../services/index';

@Component({
    selector: 'admin-panel',
    templateUrl: './app/components/admin-panel/admin-panel.html',
    styleUrls: ['./app/components/admin-panel/admin-panel.css']
})
export class AdminPanelComponent {
    private width: number = window.innerWidth * 0.8;
    private tabs: string[] = ['Calibration', 'Stability', 'Actions', 'Camera'];
    private activeTab = this.tabs[0];

    constructor(private socketio: SocketIO) {
        socketio.connect(true);
    }

    private setCorner(corner: number) {
        this.socketio.sendMessage('admin-cmd-set-corner', corner);
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
}
