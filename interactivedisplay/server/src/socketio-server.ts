import * as socketio from 'socket.io';

import { SocketIOMessage } from './socketio-message';
import { SocketIOMessageListener } from './socketio-message-listener';
import { WebServer } from './web-server';
import { Util } from './util';

const LOG_PREFIX = "[Display] ";

export class SocketIoServer {

    private _ioServer: SocketIO.Server;
    private _clients: SocketIO.Socket[] = [];
    private _msgListeners: SocketIOMessageListener[] = [];

    public Start(webserver: WebServer): void {
        this._ioServer = socketio(webserver.GetServer());

        this._ioServer.on('connection', (socket) => {
            console.log("New socket.io client");
        });

        console.log("Successfully attached SocketIO to webserver");
    }

    public Stop(): void {
        this._ioServer.close();
    }

    public Broadcast(channel: string, msg: any): void {
        for (let client of this._clients) {
            client.emit(channel, msg);
        }
    }

    public OnMessageReceived(listener: SocketIOMessageListener): void {
        this._msgListeners.push(listener);
    }

    private RaiseMessageReceivedEvent(msg: SocketIOMessage): void {
        for (let listener of this._msgListeners) {
            listener.handler(msg);
        }
    }

    private HandleConnection(socket: SocketIO.Socket) {
        this.HandleNewConnection(socket);

        socket.on('disconnect', () => {
            this.HandleSocketDisconnect(socket);
        });
    }

    private HandleNewConnection(socket: SocketIO.Socket) {
        console.log(LOG_PREFIX + "New SocketIO client connected from " + socket.id);
        this._clients.push(socket);
    }

    private HandleSocketDisconnect(socket: SocketIO.Socket) {
        console.log(LOG_PREFIX + "Client " + socket.id +  " disconnect");
        _.pull(this._clients, socket);
    }
}