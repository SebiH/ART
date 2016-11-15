import { UnityServer } from "./unity-server";
import { UnityMessageListener } from './unity-message-listener';
import { WebServer } from "./web-server";
import { SocketIOServer } from "./socketio-server";
import { SocketIOMessageListener } from './socketio-message-listener';

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

let unityServer = new UnityServer();
unityServer.start(UNITY_PORT);

let webServer = new WebServer();
webServer.start(WEB_PORT);

let sioServer = new SocketIOServer();
sioServer.start(webServer);

sioServer.onMessageReceived({
    handler: (msg) => {
        unityServer.broadcast(JSON.stringify(msg));
    }
});

unityServer.onMessageReceived({
    handler: (msg) => {
        sioServer.broadcast(msg.command, msg.payload, msg.target);
    }
})
