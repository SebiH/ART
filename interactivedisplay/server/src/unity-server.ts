import * as net from 'net';
import * as _ from 'lodash';

import { UnityMessage } from './unity-message';
import { UnityMessageListener } from './unity-message-listener';
import { Util } from './util';

const LOG_PREFIX = "[Unity] ";

export class UnityServer {
    private _msgListeners: UnityMessageListener[] = [];
    private _clients: net.Socket[] = [];
    private _server: net.Server;

    public Start(port: number): void {
        this._server = net.createServer((socket) => this.HandleConnection(socket));
        this._server.listen(port);
        console.log('Unity server listening on ' + Util.GetIp() + ':' + port);
    }


    public Stop(): void {
        this._server.close();
    }


    public Broadcast(msg: string) {
        for (let client of this._clients) {
            client.write(msg);
        }
    }

    public OnMessageReceived(msg: UnityMessage): void {
        for (let listener of this._msgListeners) {
            listener.handler(msg);
        }
    }


    private HandleConnection(socket: net.Socket) {
        this.HandleNewConnection(socket);

        socket.on('data', (data) => {
            this.HandleSocketData(socket, data);
        });

        socket.on('error', (error) => {
            this.HandleSocketError(socket, error);
        });

        socket.on('end', () => {
            this.HandleSocketDisconnect(socket);
        });
    }

    private HandleNewConnection(socket: net.Socket) {
        console.log(LOG_PREFIX + "New unity client connected from " + socket.address);
        this._clients.push(socket);
    }

    private HandleSocketData(socket: net.Socket, data: Buffer) {
        console.log(LOG_PREFIX + "" + data);
    }

    private HandleSocketError(socket: net.Socket, error: Error) {
        console.error(LOG_PREFIX + error.message);
        console.error(LOG_PREFIX + error.stack);
        _.pull(this._clients, socket);
    }

    private HandleSocketDisconnect(socket: net.Socket) {
        console.log(LOG_PREFIX + "Client " + socket.address +  " disconnect");
        _.pull(this._clients, socket);
    }
}
