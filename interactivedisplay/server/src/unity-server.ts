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
        let msgBytes = this.toUTF8Array(msg);
        for (let client of this.clients) {
            // message format:
            // \0\0\0 (Packet header as string) \0 (Actual packet json string)
            client.write('\0\0\0' + msgBytes.length.toString() + '\0');
            client.write(msg);
        }
    }

    // adapted from http://stackoverflow.com/a/18729931
    private toUTF8Array(str: string): number[] {
        let utf8: number[] = [];

        for (let i = 0; i < str.length; i++) {
            let charcode = str.charCodeAt(i);
            if (charcode < 0x80) { utf8.push(charcode); }
            else if (charcode < 0x800) {
                utf8.push(0xc0 | (charcode >> 6), 
                    0x80 | (charcode & 0x3f));
            }
            else if (charcode < 0xd800 || charcode >= 0xe000) {
                utf8.push(0xe0 | (charcode >> 12), 
                    0x80 | ((charcode>>6) & 0x3f), 
                    0x80 | (charcode & 0x3f));
            }
            // surrogate pair
            else {
                i++;
                // UTF-16 encodes 0x10000-0x10FFFF by
                // subtracting 0x10000 and splitting the
                // 20 bits of 0x0-0xFFFFF into two halves
                charcode = 0x10000 + (((charcode & 0x3ff)<<10)
                    | (str.charCodeAt(i) & 0x3ff));
                utf8.push(0xf0 | (charcode >>18), 
                    0x80 | ((charcode>>12) & 0x3f), 
                    0x80 | ((charcode>>6) & 0x3f), 
                    0x80 | (charcode & 0x3f));
            }
        }
        return utf8;
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
