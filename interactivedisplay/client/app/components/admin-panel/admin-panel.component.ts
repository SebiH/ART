import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
    selector: 'admin-panel',
    templateUrl: './app/components/admin-panel/admin-panel.html',
    styleUrls: ['./app/components/admin-panel/admin-panel.css']
})
export class AdminPanelComponent implements OnInit, OnDestroy {

    private width: number = window.innerWidth * 0.8;

    ngOnInit() {
    }

    ngOnDestroy() {

    }
}
