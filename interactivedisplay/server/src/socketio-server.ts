import * as socketio from 'socket.io';

import { WebServer } from './web-server';
import { Util } from './util';

export class SocketIoServer {

    private _ioServer;

    public Start(webserver: WebServer): void {
        this._ioServer = socketio(webserver.GetServer());

        this._ioServer.on('connection', (socket) => {
            console.log("New socket.io client");
        });

        console.log("Successfully attached SocketIO to webserver");
    }

    public Stop(): void {

    }

    public Broadcast(msg: string): void {

    }

    public OnMessageReceived(): void {

    }
}