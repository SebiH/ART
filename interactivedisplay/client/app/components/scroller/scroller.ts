import { Component, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { SocketIO } from '../../services/index';

@Component({
  selector: 'pc-scroll',
  templateUrl: './app/components/scroller/scroller.html',
  styleUrls: ['./app/components/scroller/scroller.css'],
})
export class ScrollerComponent implements OnInit, OnDestroy {

    constructor(
        private elementRef: ElementRef)
    {}

    ngOnInit() {

    }

    ngOnDestroy() {

    }

}
