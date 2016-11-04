import {Component, ViewEncapsulation} from '@angular/core';
import {Router} from '@angular/router';

import {MenuComponent} from '../menu/menu';

@Component({
    selector: 'main-app',
    templateUrl: './app/components/app/app.html',
    styleUrls: ['./app/components/app/app.css'],
    encapsulation: ViewEncapsulation.None
})

export class AppComponent { }
