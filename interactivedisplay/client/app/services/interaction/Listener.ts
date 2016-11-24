import {InteractionEventType} from './EventType';
import {InteractionEvent} from './Event';

export interface InteractionListener {
    element: HTMLElement,
    type: InteractionEventType,
    handler: (event: InteractionEvent) => void
}
