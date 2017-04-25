import { Component } from '@angular/core';
import { SocketIO } from '../../services/index';

@Component({
    selector: 'admin-table-setup',
    templateUrl: './app/components/admin-table-setup/admin-table-setup.html',
    styleUrls: ['./app/components/admin-table-setup/admin-table-setup.css']
})
export class AdminTableSetupComponent {
    private width: number = window.innerWidth * 0.8;
    private tabs: string[] = ['Calibration', 'Stability', 'Actions', 'Camera', 'Data'];
    private activeTab = this.tabs[0];

    constructor(private socketio: SocketIO) {
        socketio.connect(true);

        // quick hack to disable global css for main app
        document.getElementsByTagName('html')[0].style.overflow = "auto";
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
