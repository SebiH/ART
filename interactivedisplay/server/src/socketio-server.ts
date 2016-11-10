import * as socketio from 'socket.io';
import * as _ from 'lodash';

import { SocketIOMessage } from './socketio-message';
import { SocketIOMessageListener } from './socketio-message-listener';
import { WebServer } from './web-server';
import { Util } from './util';

const LOG_PREFIX = "[Display] ";

export class SocketIoServer {

    private ioServer: SocketIO.Server;
    private clients: SocketIO.Socket[] = [];
    private msgListeners: SocketIOMessageListener[] = [];

    public start(webserver: WebServer): void {
        this.ioServer = socketio(webserver.getServer());

        this.ioServer.on('connection', (socket) => {
            this.handleConnection(socket);
        });

        console.log("Successfully attached SocketIO to webserver");
    }

    public stop(): void {
        this.ioServer.close();
    }

    public broadcast(channel: string, msg: any): void {
        for (let client of this.clients) {
            client.emit(channel, msg);
        }
    }

    public onMessageReceived(listener: SocketIOMessageListener): void {
        this.msgListeners.push(listener);
    }

    private raiseMessageReceivedEvent(msg: SocketIOMessage): void {
        for (let listener of this.msgListeners) {
            listener.handler(msg);
        }
    }

    private handleConnection(socket: SocketIO.Socket): void {
        this.handleNewConnection(socket);

        socket.on('command', (data) => {
            this.handleCommand(socket, data);
        });

        socket.on('disconnect', () => {
            this.handleSocketDisconnect(socket);
        });
    }

    private handleNewConnection(socket: SocketIO.Socket): void {
        console.log(LOG_PREFIX + "New SocketIO client connected from " + socket.id);
        this.clients.push(socket);
    }

    private handleCommand(socket: SocketIO.Socket, data: any): void {
        this.raiseMessageReceivedEvent({
            command: data.command,
            origin: 'TODO', // TODO
            payload: data.payload
        });
    }

    private handleSocketDisconnect(socket: SocketIO.Socket): void {
        console.log(LOG_PREFIX + "Client " + socket.id +  " disconnect");
        _.pull(this.clients, socket);
    }
}