import { UnityServer } from "./unity-server";
import { UnityMessageListener } from './unity-message-listener';
import { WebServer } from "./web-server";
import { SocketIOServer } from "./socketio-server";
import { SocketIOMessageListener } from './socketio-message-listener';
import { GraphDataProvider } from './graph-data-provider';
import { GraphStorage } from './graph-storage';

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

let unityServer = new UnityServer();
unityServer.start(UNITY_PORT);


let webServer = new WebServer(WEB_PORT);

let graphDataProvider = new GraphDataProvider('');
webServer.addPath('/api/graph/data', (req, res, next) => {
    let params = req.body;
    res.json(graphDataProvider.getData(params['dimension']));
});

webServer.addPath('/api/graph/dimensions', (req, res, next) => {
    res.json(graphDataProvider.getDimensions());
});



let sioServer = new SocketIOServer();

sioServer.onMessageReceived({
    handler: (msg) => {
        unityServer.broadcast(JSON.stringify(msg));
    }
});

unityServer.onMessageReceived({
    handler: (msg) => {
        sioServer.broadcast(msg.command, msg.payload, msg.target);
    }
});



let graphStorage = new GraphStorage();

webServer.addPath('/api/graph/list', (req, res, next) => {
    res.json(graphStorage.getGraphs());
});

sioServer.onMessageReceived({
    handler: (msg) => {
        switch (msg.command) {
            case '+graph':
            case 'graph-data':
            case 'graph-position':
                graphStorage.setGraph(JSON.parse(msg.payload));
                break;
            case '-graph':
                graphStorage.removeGraph(<number>msg.payload);
        }
    }
})


webServer.start();
sioServer.start(webServer);
