import { Component, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import * as _ from 'lodash';
import { SocketIO, InteractionListener, InteractionEventType, InteractionManager, InteractionEvent } from '../../services/index';
import { Point } from '../../models/index';

const INERTIA_RATE = 0.95;

const MIN_ZOOM = 0.1;
const MAX_ZOOM = 5;


@Component({
  selector: 'pan-zoom',
  templateUrl: './app/components/panzoom/panzoom.html',
  styleUrls: ['./app/components/panzoom/panzoom.css'],
})
export class PanZoomComponent implements OnInit, OnDestroy {

    private position: Point = new Point(0, 0);
    private zoom: number = 1;

    private intertiaVelocity: Point = new Point(0, 0);

    private panzoomStartListener: InteractionListener;
    private panzoomUpdateListener: InteractionListener;
    private panzoomStopListener: InteractionListener;

    constructor (
        private socketio: SocketIO,
        private elementRef: ElementRef,
        private interactions: InteractionManager
        ) {}

    ngOnInit() {
        this.panzoomStartListener = {
            type: InteractionEventType.PanZoomStart,
            element: this.elementRef.nativeElement,
            handler: (ev) => { return this.onPanZoomStart(ev); }
        };
        this.interactions.on(this.panzoomStartListener);


        this.panzoomUpdateListener = {
            type: InteractionEventType.PanZoomUpdate,
            element: this.elementRef.nativeElement,
            handler: (ev) => { return this.onPanZoomUpdate(ev); }
        };
        this.interactions.on(this.panzoomUpdateListener);

        this.panzoomStopListener = {
            type: InteractionEventType.PanZoomEnd,
            element: this.elementRef.nativeElement,
            handler: (ev) => { return this.onPanZoomEnd(ev); }
        };
        this.interactions.on(this.panzoomStopListener);
    }

    ngOnDestroy() {
        this.interactions.off(this.panzoomStartListener);
        this.interactions.off(this.panzoomUpdateListener);
        this.interactions.off(this.panzoomStopListener);
    }


    private onPanZoomStart(ev: InteractionEvent): boolean {
        this.intertiaVelocity = new Point(0, 0);
        return false;
    }

    private onPanZoomUpdate(ev: InteractionEvent): boolean {
        if (ev.scale == 1) {
            this.handlePan(ev);
        } else {
            this.handleZoom(ev);
        }

        this.intertiaVelocity = ev.delta;

        return false;
    }

    private onPanZoomEnd(ev: InteractionEvent): boolean {
        this.applyInertia();
        return false;
    }



    private handlePan(ev: InteractionEvent) {
        this.position.x = this.position.x + ev.delta.x / this.zoom;
        this.position.y = this.position.y + ev.delta.y / this.zoom;
    }



    private handleZoom(ev: InteractionEvent) {
        let zoomScale = ev.scale;
        let newZoomLvl = this.zoom * ev.scale;

        if (newZoomLvl < MIN_ZOOM) {
            zoomScale = MIN_ZOOM / this.zoom;
            newZoomLvl = MIN_ZOOM;
        } else if (newZoomLvl > MAX_ZOOM) {
            zoomScale = MAX_ZOOM / this.zoom;
            newZoomLvl = MAX_ZOOM;
        }

        var adjustX = ev.prevCenter.x * (zoomScale - 1);
        var adjustY = ev.prevCenter.y * (zoomScale - 1);

        this.position.x = this.position.x + (ev.delta.x + adjustX) / this.zoom;
        this.position.y = this.position.y + (ev.delta.y + adjustY) / this.zoom;
        this.zoom = newZoomLvl;
    }


    private applyInertia(): void {
        this.intertiaVelocity = this.intertiaVelocity.multiplyNum(INERTIA_RATE);

        this.position.x += this.intertiaVelocity.x;
        this.position.y += this.intertiaVelocity.y;

        if (Math.abs(this.intertiaVelocity.x) > 0.001 || Math.abs(this.intertiaVelocity.y) > 0.001) {
           setTimeout(() => { this.applyInertia(); }, 10);
        }
    }



    private viewStyle = {
        transform: 'scale(1)',
        'transform-origin': '0px 0px 0px'
    };

    getViewStyle() {
        let values = [
            'translate3d(' + -this.position.x + 'px, ' + -this.position.y + 'px, 0)',
            'scale(' + this.zoom + ', ' + this.zoom + ')',
        ];

        this.viewStyle.transform = values.join(' ');
        this.viewStyle['transform-origin'] = this.position.x + 'px ' + this.position.y + 'px 0px';

        return this.viewStyle;
    }

 
}
