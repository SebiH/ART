import { Component, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import * as _ from 'lodash';
import { SocketIO } from '../../services/index';

import { InteractionData } from './InteractionData';

@Component({
  selector: 'pan-zoom',
  templateUrl: './app/components/panzoom/panzoom.html',
  styleUrls: ['./app/components/panzoom/panzoom.css'],
})
export class PanZoomComponent implements OnInit, OnDestroy {

    private _posX: number = 0;
    get posX(): number {
        return this._posX;
    }

    set posX(val: number) {
        this._posX = val;
        this.delayedSync();
    }

    private _posY: number = 0;
    get posY(): number {
        return this._posY;
    }

    set posY(val: number) {
        this._posY = val;
        this.delayedSync();
    }


    private _zoom: number = 1;
    get zoom(): number {
        return this._zoom;
    }

    set zoom(val: number) {
        this._zoom = val;
        this.delayedSync();
    }

    private delayedSync: Function;

    constructor(private socketio: SocketIO, private elementref: ElementRef) {
        this.delayedSync = _.debounce(() => {this.sync();});
    }

    ngOnInit() {
        let el = this.elementref.nativeElement;
        this.addMouseInteraction(el);
        this.addTouchInteraction(el);
    }


    ngOnDestroy() {
        let el = this.elementref.nativeElement;
        el.ontouchstart = null;
        el.ontouchmove = null;
        el.ontouchend = null;
        el.ontouchcancel = null;

        el.onmousedown = null;
        el.onmousemove = null;
        el.onmousewheel = null;
        el.onmouseup = null;
    }

    private sync() {
        this.socketio.sendMessage('panzoom', {
            posX: this.posX,
            posY: this.posY,
            zoom: this.zoom
        });
    }


    private viewStyle = {
        transform: 'scale(1)',
        'transform-origin': '0px 0px 0px'
    };

    getViewStyle() {
        let values = [
            'translate3d(' + -this.posX + 'px, ' + -this.posY + 'px, 0)',
            'scale(' + this.zoom + ', ' + this.zoom + ')',
        ];

        this.viewStyle.transform = values.join(' ');
        this.viewStyle['transform-origin'] = this.posX + 'px ' + this.posY + 'px 0px';

        return this.viewStyle;
    }


    applyInertia(x: number, y: number): void {

        if (this.isMouseDown || _.values(this.activeTouches).length > 0) {
            return;
        }

        x *= 0.9;
        y *= 0.9;

        this.posX += x;
        this.posY += y;

        if (Math.abs(x) < 0.01) {
            x = 0;
        }

        if (Math.abs(y) < 0.01) {
            y = 0;
        }

        if (Math.abs(x) > 0 || Math.abs(y) > 0) {
            setTimeout(() => { this.applyInertia(x, y) }, 10);
        }
    }


    /*
     *    Mouse Handling
     */

    private mouseData: InteractionData = new InteractionData(0, 0);
    private isMouseDown: boolean;

    private addMouseInteraction(el: HTMLElement): void {
        el.onmousedown = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onMouseDown(el, ev);
        }

        el.onmousemove = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onMouseMove(el, ev);
        }

        el.onmouseup = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onMouseUp(el, ev);
        }

        el.onmousewheel = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onMouseWheel(el, ev);
        }
    }




    private onMouseDown(el: HTMLElement, ev: MouseEvent): void {
        if (ev.button === 0) {
            this.isMouseDown = true;
            this.mouseData = new InteractionData(ev.clientX, ev.clientY);
        }
    }




    private onMouseMove(el: HTMLElement, ev: MouseEvent): void {

        if (this.isMouseDown) {
            this.mouseData.setPos(ev.clientX, ev.clientY);
            let deltaX = this.mouseData.prevPosX - this.mouseData.currPosX;
            this.posX += (deltaX / this.zoom);

            let deltaY = this.mouseData.prevPosY - this.mouseData.currPosY;
            this.posY += (deltaY / this.zoom);
        }
    }


    private onMouseUp(el: HTMLElement, ev: MouseEvent): void {
        if (ev.button === 0) {
            this.isMouseDown = false;
            let deltaX = this.mouseData.prevPosX - this.mouseData.currPosX;
            let inertiaX = (deltaX / this.zoom);

            let deltaY = this.mouseData.prevPosY - this.mouseData.currPosY;
            let inertiaY = (deltaY / this.zoom);

            setTimeout(() => { this.applyInertia(inertiaX, inertiaY) });
        }
    }



    private onMouseWheel(el: HTMLElement, ev: MouseWheelEvent): void {
        let scale = ev.wheelDelta < 0 ? 0.975 : 1.025;
        this.zoom *= scale;
    }



    /*
     *    Touch Handling
     */



    private activeTouches: {[identifier: number]: InteractionData} = {};

    private addTouchInteraction(el: HTMLElement): void {
        el.ontouchstart = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onTouchStart(el, ev);
        };

        el.ontouchmove = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onTouchMove(ev);
        }

        el.ontouchend = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onTouchEnd(ev);
        }

        el.ontouchcancel = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onTouchEnd(ev);
        }
    }



    private onTouchStart(el: HTMLElement, ev: TouchEvent): void {
        for (let i = 0; i < ev.changedTouches.length; i++) {
            let touch = ev.changedTouches[i];
            let interaction = new InteractionData(touch.screenX, touch.screenY);
            this.activeTouches[touch.identifier] = interaction;
        }
    }


    private onTouchMove(ev: TouchEvent): void {

        for (let i = 0; i < ev.changedTouches.length; i++) {
            let touch = ev.changedTouches[i];
            let interaction = this.activeTouches[touch.identifier];

            if (!interaction) {
                console.error('No matching touch identifier found, didn\'t receive onTouchStart??');
            } else {
                interaction.setPos(touch.screenX, touch.screenY);
            }
        }


        let currentCenterX = 0
        let currentCenterY = 0;

        let prevCenterX = 0;
        let prevCenterY = 0;

        let touches: InteractionData[] = _.values<InteractionData>(this.activeTouches);

        for (let touch of touches) {
            currentCenterX += touch.currPosX;
            currentCenterY += touch.currPosY;

            prevCenterX += touch.prevPosX;
            prevCenterY += touch.prevPosY;
        }

        currentCenterX = currentCenterX / touches.length;
        currentCenterY = currentCenterY / touches.length;
        prevCenterX = prevCenterX / touches.length;
        prevCenterY = prevCenterY / touches.length;

        if (touches.length === 1) { // pan-only
            let interaction = touches[0];

            let deltaX = interaction.prevPosX - interaction.currPosX;
            this.posX += (deltaX / this.zoom);

            let deltaY = interaction.prevPosY - interaction.currPosY;
            this.posY += (deltaY / this.zoom);

        } else if (touches.length === 2) { // pan-zoom

            let deltaCenterX = prevCenterX - currentCenterX;
            let deltaCenterY = prevCenterY - currentCenterY;

            let t1 = touches[0];
            let t2 = touches[1];

            let prevDistance = Math.sqrt(Math.pow(t1.prevPosX - t2.prevPosX, 2) + Math.pow(t1.prevPosY - t2.prevPosY, 2))
            let currDistance = Math.sqrt(Math.pow(t1.currPosX - t2.currPosX, 2) + Math.pow(t1.currPosY - t2.currPosY, 2))

            let scale = currDistance / prevDistance;
            this.zoom *= scale;
        } // TODO: 3+ multi-finger zoom?

    }


    private onTouchEnd(ev: TouchEvent): void {

        for (let i = 0; i < ev.changedTouches.length; i++) {
            let touch = ev.changedTouches[i];

            let interaction = this.activeTouches[touch.identifier];

            if (!interaction) {
                console.error('No matching touch identifier found, didn\'t receive onTouchStart??');
                continue;
            }

            interaction.setPos(touch.screenX, touch.screenY);

            delete this.activeTouches[touch.identifier];

            let touches = _.values<InteractionData>(this.activeTouches);
            if (touches.length == 0) {
                let deltaX = interaction.prevPosX - interaction.currPosX;
                let inertiaX = (deltaX / this.zoom);

                let deltaY = interaction.prevPosY - interaction.currPosY;
                let inertiaY = (deltaY / this.zoom);
                setTimeout(() => { this.applyInertia(inertiaX, inertiaY); });
            }
        }
    }

}
