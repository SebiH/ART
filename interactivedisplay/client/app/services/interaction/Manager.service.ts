import {Injectable, NgZone} from '@angular/core';
import {Point} from '../../models/index';

import {Logger} from '../logger.service';

import {InteractionListener} from './Listener';
import {InteractionType} from './Type';
import {InteractionData} from './Data';
import {InteractionEvent} from './Event';
import {InteractionEventType} from './EventType';

import * as _ from 'lodash';

const MAX_PRESS_TIME = 300; // in ms
const MAX_PRESS_DISTANCE = 5; // in pixel
const FORBIDDEN_ELEMENTS = [ 'button', 'option', 'select' ];

@Injectable()
export class InteractionManager {

    private listeners: InteractionListener[] = [];

    // constructor(private logger: Logger) {
    private logger: Logger = new Logger(); // TODO: inject?
    constructor(private ngZone: NgZone) {

        // in case 'touchup/move' etc ends up on unregistered elements
        this.subscribeElementListeners(document.body);
    }



    /*
     *    Touch-Event registration
     */

    public on(listener: InteractionListener): void {
        // no initial listeners for this element -> add native listeners
        let hasMatchingElements = (_.findIndex(this.listeners, (l) => { return l.element == listener.element; }) >= 0);
        if (!hasMatchingElements) {
            this.subscribeElementListeners(listener.element);
        }

        this.listeners.push(listener);
    }

    public off(listener: InteractionListener): void {
        _.pull(this.listeners, listener);

        // no more listeners for this HTMLelement -> deregister listeners
        let hasMatchingElements = (_.findIndex(this.listeners, (l) => { return l.element == listener.element; }) >= 0);
        if (!hasMatchingElements) {
            this.unsubscribeElementListeners(listener.element);
        }
    }



    private raiseEvent(interaction: InteractionData, event: InteractionEvent): boolean {

        let hasListeners = false;

        for (let listener of this.listeners) {
            let matchesType = event.type === listener.type;
            let matchesElement = interaction.element === listener.element;
            let isPanZoom = interaction.type === InteractionType.PanZoom;

            if (matchesType && (matchesElement || isPanZoom)) {
                listener.handler(event);
                hasListeners = true;
            }

        }

        return hasListeners;
    }




    /*
     *    HTML event registration
     */

    private subscribeElementListeners(el: HTMLElement): void {
        this.addTouchInteraction(el);
        this.addMouseInteraction(el);
    }

    private unsubscribeElementListeners(el: HTMLElement): void {
        el.ontouchstart = null;
        el.ontouchmove = null;
        el.ontouchend = null;
        el.ontouchcancel = null;

        el.onmousedown = null;
        el.onmousemove = null;
        el.onmousewheel = null;
        el.onmouseup = null;
    }





    /*
     *    Mouse Handling
     */

    private mouseData: InteractionData = new InteractionData(new Point(0, 0), false);

    private addMouseInteraction(el: HTMLElement): void {
        el.onmousedown = (ev) => {
            if (FORBIDDEN_ELEMENTS.indexOf(ev.srcElement.tagName.toLowerCase()) < 0) {
                ev.preventDefault();
                ev.stopPropagation();
                this.onMouseDown(el, ev);
            }
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
            this.mouseData.isActive = true;
            this.mouseData.element = el;
            this.mouseData.type = InteractionType.Undecided;

            let mousePos = new Point(ev.clientX, ev.clientY);
            this.mouseData.startPos = mousePos;
            this.mouseData.currPos = mousePos;
            this.mouseData.prevPos = mousePos;

            this.startPressTimer(this.mouseData);
        }
    }




    private onMouseMove(el: HTMLElement, ev: MouseEvent): void {
        let newPos = new Point(ev.clientX, ev.clientY);

        if (this.mouseData.isActive) {

            // TODO only update currentPosition, so that we can catch up with prevPosition later
            let oldPos = this.mouseData.currPos;
            this.mouseData.currPos = newPos;
            this.mouseData.addTotalDistance(Point.sub(oldPos, newPos));

            if (this.mouseData.type === InteractionType.Undecided) {
                if (this.isEligibleForPress(this.mouseData)) {
                    // do nothing
                } else {
                    // no longer eligible for press, trigger touchdown immediately
                    this.stopPressTimer(this.mouseData);
                    this.triggerTouchDown(this.mouseData);
                }
            }

            // no 'else', because interactiontype might have changed
            if (this.mouseData.type !== InteractionType.Undecided) {

                this.mouseData.prevPos = oldPos;
                let delta = Point.sub(oldPos, newPos);
                let type = (this.mouseData.type === InteractionType.PanZoom) ?
                InteractionEventType.PanZoomUpdate :
                InteractionEventType.TouchMove;

                this.raiseEvent(this.mouseData, {
                    type: type,
                    position: newPos,
                    delta: delta,
                    scale: 1,
                    prevCenter: this.mouseData.prevPos,
                    center: this.mouseData.currPos
                });
            }


        } else {
            this.mouseData.currPos = newPos;
        }
    }


    private onMouseUp(el: HTMLElement, ev: MouseEvent): void {
        if (ev.button === 0) {

            let hasTimerStopped = this.stopPressTimer(this.mouseData);
            this.mouseData.isActive = false;


            if (hasTimerStopped) {
                this.raiseEvent(this.mouseData, {
                    type: InteractionEventType.Click,
                    position: this.mouseData.currPos
                });
            } else {
                let evType: InteractionEventType;
                switch (this.mouseData.type) {
                    case InteractionType.PanZoom:
                        evType = InteractionEventType.PanZoomEnd;
                        break;

                    case InteractionType.Press:
                        evType = InteractionEventType.PressUp;
                        break;

                    case InteractionType.Touch:
                        evType = InteractionEventType.TouchUp;
                        break;
                }

                this.raiseEvent(this.mouseData, {
                    type: evType,
                    position: this.mouseData.currPos
                });

                if (Point.distanceBetween(new Point(0, 0), this.mouseData.totalDistance) < MAX_PRESS_DISTANCE) {
                    this.raiseEvent(this.mouseData, {
                        type: InteractionEventType.Click,
                        position: this.mouseData.currPos
                    })
                }

            }


            this.mouseData.type = InteractionType.Undecided;
        }
    }



    private onMouseWheel(el: HTMLElement, ev: MouseWheelEvent): void {
        let scale = ev.wheelDelta < 0 ? 0.975 : 1.025;

        if (!this.mouseData.isActive) {
            this.raiseEvent(this.mouseData, {
                type: InteractionEventType.PanZoomStart,
                position: this.mouseData.currPos
            });
        }

        this.raiseEvent(this.mouseData, {
            type: InteractionEventType.PanZoomUpdate,
            position: this.mouseData.currPos,
            center: this.mouseData.currPos,
            prevCenter: this.mouseData.currPos,
            scale: scale,
            delta: new Point(0, 0)
        });

        if (!this.mouseData.isActive) {
            this.raiseEvent(this.mouseData, {
                type: InteractionEventType.PanZoomEnd,
                position: this.mouseData.currPos
            });
        }
    }






    /*
     *    Touch Handling
     */

    private activeTouches: {[identifier: number]: InteractionData} = {};

    private addTouchInteraction(el: HTMLElement): void {
        el.ontouchstart = (ev) => {
            if (FORBIDDEN_ELEMENTS.indexOf(ev.srcElement.tagName.toLowerCase()) < 0) {
                ev.preventDefault();
                ev.stopPropagation();
                this.onTouchStart(el, ev);
            }

            this.ngZone.run(() => {});
        };

        el.ontouchmove = (ev) => {
            ev.preventDefault();
            ev.stopPropagation();
            this.onTouchMove(ev);
            this.ngZone.run(() => {});
        }

        el.ontouchend = (ev) => {
            if (FORBIDDEN_ELEMENTS.indexOf(ev.srcElement.tagName.toLowerCase()) < 0) {
                ev.preventDefault();
                ev.stopPropagation();
                this.onTouchEnd(ev);
            }
            
            this.ngZone.run(() => {});
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
            let touchPos = new Point(touch.screenX, touch.screenY);

            let interaction = new InteractionData(touchPos);
            interaction.element = el;
            this.activeTouches[touch.identifier] = interaction;

            this.startPressTimer(interaction);
        }
    }


    private onTouchMove(ev: TouchEvent): void {

        let panzoomTouches: InteractionData[] = [];

        for (let activeTouch of _.values<InteractionData>(this.activeTouches)) {
            if (activeTouch.type === InteractionType.PanZoom) {
                panzoomTouches.push(activeTouch);
                activeTouch.prevPos = activeTouch.currPos;
            }
        }

        for (let i = 0; i < ev.changedTouches.length; i++) {
            let touch = ev.changedTouches[i];
            let touchPos = new Point(touch.screenX, touch.screenY);
            let interaction = this.activeTouches[touch.identifier];

            if (!interaction) {
                this.logger.error('No matching touch identifier found, didn\'t receive onTouchStart??');
            } else {
                let prevPos = interaction.currPos;
                interaction.currPos = touchPos;
                let delta = Point.sub(prevPos, touchPos);
                interaction.addTotalDistance(delta);

                // check if movement threshold was broken if touch was waiting for press
                if (interaction.type === InteractionType.Undecided && !this.isEligibleForPress(interaction)) {
                    this.triggerTouchDown(interaction);
                    this.stopPressTimer(interaction);
                }

                if (interaction.type !== InteractionType.PanZoom && interaction.type !== InteractionType.Undecided) {
                    this.raiseEvent(interaction, {
                        type: InteractionEventType.TouchMove,
                        position: touchPos,
                        delta: delta
                    });
                }
            }
        }


        let currentCenter = new Point(0, 0);
        let prevCenter = new Point(0, 0);

        for (let pzTouch of panzoomTouches) {
            currentCenter = Point.add(currentCenter, pzTouch.currPos);
            prevCenter = Point.add(prevCenter, pzTouch.prevPos);
        }

        currentCenter = currentCenter.divideBy(panzoomTouches.length);
        prevCenter = prevCenter.divideBy(panzoomTouches.length);

        // TODO: multi-finger (3+ zoom)
        if (panzoomTouches.length === 1) { // pan-only
            let interaction = panzoomTouches[0];
            let delta = Point.sub(interaction.prevPos, interaction.currPos);

            this.raiseEvent(interaction, {
                type: InteractionEventType.PanZoomUpdate,
                position: interaction.currPos,
                center: currentCenter,
                prevCenter: prevCenter,
                delta: delta,
                scale: 1
            });
        } else if (panzoomTouches.length === 2) { // pan-zoom

            let deltaCenter = Point.sub(prevCenter, currentCenter);
            let prevDistance = panzoomTouches[0].prevPos.distanceTo(panzoomTouches[1].prevPos);
            let currDistance = panzoomTouches[0].currPos.distanceTo(panzoomTouches[1].currPos);
            let scale = currDistance / prevDistance;

            this.raiseEvent(panzoomTouches[0], {
                type: InteractionEventType.PanZoomUpdate,
                position: currentCenter,
                center: currentCenter,
                prevCenter: prevCenter,
                scale: scale,
                delta: deltaCenter
            });
        } // TODO: 3+ multi-finger zoom?

    }


    private onTouchEnd(ev: TouchEvent): void {

        let hasTriggeredPanEnd: boolean = false;

        for (let i = 0; i < ev.changedTouches.length; i++) {
            let touch = ev.changedTouches[i];
            let touchPos = new Point(touch.screenX, touch.screenY);

            let interaction = this.activeTouches[touch.identifier];

            if (!interaction) {
                this.logger.error('No matching touch identifier found, didn\'t receive onTouchStart??');
                continue;
            }

            interaction.isActive = false;
            let hasTimerStopped = this.stopPressTimer(interaction);

            if (hasTimerStopped) {
                this.raiseEvent(interaction, {
                    type: InteractionEventType.Click,
                    position: touchPos
                });
            } else {
                if (interaction.type === InteractionType.Undecided) {
                    // need to send a 'start' event before sending an 'end' event
                    this.triggerTouchDown(interaction);
                }

                if (interaction.type === InteractionType.Press || interaction.type === InteractionType.Touch) {
                    let eventType = (interaction.type === InteractionType.Press) ? InteractionEventType.PressUp : InteractionEventType.TouchUp;
                    this.raiseEvent(interaction, {
                        type: eventType,
                        position: touchPos
                    });

                    // if event was press, but no listener was registered, fire a click event just in case
                    if (Point.distanceBetween(new Point(0, 0), interaction.totalDistance) < MAX_PRESS_DISTANCE) {
                        this.raiseEvent(interaction, {
                            type: InteractionEventType.Click,
                            position: touchPos
                        })
                    }
                }
            }

            delete this.activeTouches[touch.identifier];

            let hasActivePanZoomTouches = _(this.activeTouches)
                .values<InteractionData>()
                .findIndex((t) => { return t.type === InteractionType.PanZoom; }) > -1;


            if (!hasActivePanZoomTouches) {
                hasTriggeredPanEnd = true;

                this.raiseEvent(interaction, {
                    type: InteractionEventType.PanZoomEnd,
                    position: touchPos
                });
            }
        }
    }



    /*
     *    Press Handling
     */

    private startPressTimer(interaction: InteractionData): void {

        // see if there's even a press handler registered
        let pressListeners: InteractionListener[] = [];
        let clickListeners: InteractionListener[] = [];

        for (let listener of this.listeners) {
            let isPressEvent = (listener.type === InteractionEventType.PressDown || listener.type === InteractionEventType.PressUp);
            let isClickEvent = (listener.type === InteractionEventType.Click);

            if (listener.element == interaction.element) {
                if (isPressEvent) {
                    pressListeners.push(listener);
                } else if (isClickEvent) {
                    clickListeners.push(listener);
                }
            }
        }

        if (pressListeners.length > 0 || clickListeners.length > 0) {
            interaction.timeoutId = window.setTimeout(() => {
                interaction.timeoutId = -1;
                if (interaction.isActive && this.isEligibleForPress(interaction)) {
                    if (pressListeners.length > 0) {
                        this.triggerPressDown(interaction);
                    }
                } else {
                    this.triggerTouchDown(interaction);
                }

            }, MAX_PRESS_TIME);
        } else {
            this.triggerTouchDown(interaction);
        }
    }

    // returns if a timer was stopped
    private stopPressTimer(interaction: InteractionData): boolean {
        if (interaction.timeoutId !== -1) {
            window.clearTimeout(interaction.timeoutId);
            interaction.timeoutId = -1;
            return true;
        }

        return false;
    }


    private triggerPressDown(interaction: InteractionData): void {
        interaction.type = InteractionType.Press;

        this.raiseEvent(interaction, {
            type: InteractionEventType.PressDown,
            position: interaction.currPos
        });
    }

    private triggerTouchDown(interaction: InteractionData): void {
        interaction.type = InteractionType.Touch;

        let hasTriggeredEvents = this.raiseEvent(interaction, {
            type: InteractionEventType.TouchDown,
            position: interaction.currPos
        });

        if (!hasTriggeredEvents) {
            interaction.type = InteractionType.PanZoom;
            this.raiseEvent(interaction, {
                type: InteractionEventType.PanZoomStart,
                position: interaction.currPos,
                center: interaction.currPos
            });
        }
    }

    private isEligibleForPress(interaction: InteractionData): boolean {
        return interaction.currPos.distanceTo(interaction.startPos) < MAX_PRESS_DISTANCE;
    }
}
