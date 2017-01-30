import { Component } from '@angular/core';

@Component({
    selector: 'admin-panel',
    templateUrl: './app/components/admin-panel/admin-panel.html',
    styleUrls: ['./app/components/admin-panel/admin-panel.css']
})
export class AdminPanelComponent {
    private width: number = window.innerWidth * 0.8;
    private tabs: string[] = ['Calibration', 'Stability', 'Actions', 'Camera'];
    private activeTab = this.tabs[0];
}

