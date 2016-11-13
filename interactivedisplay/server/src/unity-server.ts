import * as net from 'net';
import * as _ from 'lodash';

import { UnityMessage } from './unity-message';
import { UnityMessageListener } from './unity-message-listener';
import { Util } from './util';

const LOG_PREFIX = "[Unity] ";

export class UnityServer {
    private msgListeners: UnityMessageListener[] = [];
    private clients: net.Socket[] = [];
    private server: net.Server;

    public start(port: number): void {
        this.server = net.createServer((socket) => this.handleConnection(socket));
        this.server.listen(port);
        console.log('Unity server listening on ' + Util.getIp() + ':' + port);
    }


    public stop(): void {
        this.server.close();
    }


    public broadcast(msg: string): void {
        for (let client of this.clients) {
            client.write(msg);
        }
    }

    public onMessageReceived(listener: UnityMessageListener): void {
        this.msgListeners.push(listener);
    }

    private raiseMessageReceivedEvent(msg: UnityMessage): void {
        console.log(msg.toString());
        for (let listener of this.msgListeners) {
            listener.handler(msg);
        }
    }


    private handleConnection(socket: net.Socket): void {
        this.handleNewConnection(socket);

        socket.on('data', (data) => {
            this.handleSocketData(socket, data);
        });

        socket.on('error', (error) => {
            this.handleSocketError(socket, error);
        });

        socket.on('end', () => {
            this.handleSocketDisconnect(socket);
        });
    }

    private handleNewConnection(socket: net.Socket): void {
        console.log(LOG_PREFIX + "New unity client connected from " + socket.address);
        this.clients.push(socket);
    }

    private handleSocketData(socket: net.Socket, data: Buffer): void {
        console.log(LOG_PREFIX + "" + data);
        // TODO.
    }

    private handleSocketError(socket: net.Socket, error: Error): void {
        console.error(LOG_PREFIX + error.message);
        console.error(LOG_PREFIX + error.stack);
        _.pull(this.clients, socket);
    }

    private handleSocketDisconnect(socket: net.Socket): void {
        console.log(LOG_PREFIX + "Client " + socket.address +  " disconnect");
        _.pull(this.clients, socket);
    }
}
