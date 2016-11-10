import { UnityServer } from './unity-server';
import { WebServer } from './web-server';
import { SocketIoServer } from './socketio-server';

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

let g_unityServer = new UnityServer();
g_unityServer.Start(UNITY_PORT);

let g_webServer = new WebServer();
g_webServer.Start(WEB_PORT);

let g_sioServer = new SocketIoServer();
g_sioServer.Start(g_webServer);
