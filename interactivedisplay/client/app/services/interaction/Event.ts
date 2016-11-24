import {Point} from '../../models/index';

import {InteractionEventType} from './EventType';

export interface InteractionEvent {
    type: InteractionEventType,

    // for pan/zoom events
    delta?: Point,
    // for zoom events
    center?: Point,
    prevCenter?: Point,
    scale?: number,

    // Position of the TouchEvent, relative to display
    position: Point
}
