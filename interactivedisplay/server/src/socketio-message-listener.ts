import { SocketIOMessage } from './socketio-message';

export interface SocketIOMessageListener {
    handler: (ev: SocketIOMessage) => void;
}
