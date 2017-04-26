import { Component } from '@angular/core';
import { SocketIO, InteractionSettings } from '../../services/index';

@Component({
    selector: 'admin-panel',
    templateUrl: './app/components/admin-panel/admin-panel.html',
    styleUrls: ['./app/components/admin-panel/admin-panel.css']
})
export class AdminPanelComponent {
    private width: number = window.innerWidth * 0.8;
    private tabs: string[] = ['Calibration', 'Stability', 'Actions', 'Camera', 'Data'];
    private activeTab = this.tabs[0];

    constructor(private socketio: SocketIO) {
        socketio.connect(true);

        // quick hack to disable global css for main app
        document.getElementsByTagName('html')[0].style.overflow = "auto";
        InteractionSettings.CatchInteractionOnBody = false;
    }
}
