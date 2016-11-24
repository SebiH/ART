export enum InteractionEventType {
    // 1 finger
    TouchDown,
    TouchMove,
    TouchUp,

    // 1 finger with initial holding down
    PressDown,
    PressUp,

    // 1/2 Fingers - Sent after TouchMove, only if no TouchMove event
    // cancelled subsequent events via returning true
    PanZoomStart,
    PanZoomUpdate,
    PanZoomEnd
}
