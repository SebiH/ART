import { Injectable } from '@angular/core';
import * as io from 'socket.io-client';

@Injectable()
export class SocketIO {
    private isDebug: boolean = false;
    private socket: SocketIOClient.Socket = undefined;
    private subscriptions: { [channel: string]: Function[]; } = {};

    constructor() {
        this.socket = io.connect();
    }

    public connect(debug: boolean = false) {
        // avoid unecessary reconnects
        if (this.isDebug !== debug) {

            this.isDebug = debug;

            if (this.socket.connected) {
                this.socket.disconnect();
            }

            if (debug) {
                this.socket = io.connect({ query: 'debug=true' });
            } else {
                this.socket = io.connect();
            }
        }
    }

    public on(name: string, onMsgReceived: Function): void {
        this.socket.on(name, onMsgReceived);
    }

    public off(name: string, onMsgReceived: Function): void {
        this.socket.off(name, onMsgReceived);
    }

    public sendMessage(command: string, data: any): void {
        this.socket.emit('command', {
            origin: 'Surface',
            command: command,
            payload: data
        });
    }
}
