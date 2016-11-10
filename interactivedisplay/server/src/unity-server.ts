import * as net from 'net';
import * as _ from 'lodash';

export class UnityServer {

    private clients: net.Socket[] = [];

    public Start(port: number): void {
        net.createServer((socket) => {
            socket.write('Hello, unity!');

            socket.on('data', (data) => {
                console.log("" + data);
            });

            socket.on('error', (err) => {
                console.log("" + err);
            });

            socket.on('end', () => {

            });

        }).listen(port);

        console.log('TCP Server listening on ' + port);
    }


    public Stop(): void {

    }


    public Broadcast(msg: string) {

    }

    public OnMessageReceived(): void {

    }
}
