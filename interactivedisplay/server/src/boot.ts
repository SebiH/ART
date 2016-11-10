import { UnityServer } from './unity-server';
import { WebServer } from './web-server';

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

var unityServer = new UnityServer();
unityServer.Start(UNITY_PORT);

var webServer = new WebServer();
webServer.Start(WEB_PORT);
