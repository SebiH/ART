import { UnityServer } from "./unity-server";
import { WebServer } from "./web-server";
import { SocketIoServer } from "./socketio-server";

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

let unityServer = new UnityServer();
unityServer.start(UNITY_PORT);

let webServer = new WebServer();
webServer.start(WEB_PORT);

let sioServer = new SocketIoServer();
sioServer.start(webServer);
