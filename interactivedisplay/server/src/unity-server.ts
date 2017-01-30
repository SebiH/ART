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
        for (let listener of this.msgListeners) {
            listener.handler(msg);
        }
    }


    private handleConnection(socket: net.Socket): void {
        let address = socket.address().address;
        console.log(LOG_PREFIX + "New unity client connected from " + address);

        this.handleNewConnection(socket);

        socket.on('data', (data) => {
            this.handleSocketData(socket, data);
        });

        socket.on('error', (error) => {
            this.handleSocketError(socket, error);
        });

        socket.on('end', () => {
            console.log(LOG_PREFIX + "Client " + address +  " disconnect");
            this.handleSocketDisconnect(socket);
        });
    }

    private handleNewConnection(socket: net.Socket): void {
        this.clients.push(socket);
    }

    private handleSocketData(socket: net.Socket, data: Buffer): void {
        let msgs = this.splitJson(data + '');

        for (let msgText of msgs) {
            let msg = <UnityMessage>JSON.parse(msgText);
            this.raiseMessageReceivedEvent(msg)
        }
    }

    private handleSocketError(socket: net.Socket, error: Error): void {
        console.error(LOG_PREFIX + error.message);
        console.error(LOG_PREFIX + error.stack);
        _.pull(this.clients, socket);
    }

    private handleSocketDisconnect(socket: net.Socket): void {
        _.pull(this.clients, socket);
    }


    // TODO: breaks easily, but sufficient for current purpose
    // TODO: see equivalent implementation in unity listener InteractiveSurfaceClient.cs
    private splitJson(text: string): string[] {
        let jsonPackets: string[] = [];

        let leftBracketIndex = -1;
        let rightBracketIndex = -1;

        let bracketCounter = 0;

        for (let i = 0; i < text.length; i++) {
            let ch = text.charAt(i);

            if (ch === '{') {
                if (bracketCounter == 0) {
                    leftBracketIndex = i;
                }

                bracketCounter++;
            } else if (ch === '}') {
                bracketCounter--;

                if (bracketCounter <= 0) {
                    rightBracketIndex = i;
                    bracketCounter = 0;

                    jsonPackets.push(text.substring(leftBracketIndex, rightBracketIndex + 1));
                }
            }
        }

        return jsonPackets;
    }
}
