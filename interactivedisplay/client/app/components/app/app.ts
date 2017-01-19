import {Component, ViewEncapsulation, OnInit, HostListener} from '@angular/core';
import {Router} from '@angular/router';

import {MenuComponent} from '../menu/menu';
import {SurfaceProvider} from '../../services/index';

@Component({
    selector: 'main-app',
    templateUrl: './app/components/app/app.html',
    styleUrls: ['./app/components/app/app.css'],
    encapsulation: ViewEncapsulation.None
})

export class AppComponent implements OnInit
{
    constructor (private surfaceProvider: SurfaceProvider) { }

    ngOnInit() {
    }

    @HostListener('window:resize', ['$event'])
    private onResize(event) {
        this.surfaceProvider.setSize(window.innerWidth, window.innerHeight);
    }
}
