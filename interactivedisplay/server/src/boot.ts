import { UnityServer } from "./unity-server";
import { UnityMessageListener } from './unity-message-listener';
import { WebServer } from "./web-server";
import { SocketIOServer } from "./socketio-server";
import { SocketIOMessageListener } from './socketio-message-listener';
import { GraphDataProvider } from './graph-data-provider';
import { ObjectStorage } from './object-storage';

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

let unityServer = new UnityServer();
unityServer.start(UNITY_PORT);


let webServer = new WebServer(WEB_PORT);

let graphDataProvider = new GraphDataProvider();
webServer.addPath('/api/graph/data', (req, res, next) => {
    let params = req.body;

    graphDataProvider.getData(params['dimension'], (data) => {
        res.json(data);
    });
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



let graphStorage = new ObjectStorage();

webServer.addPath('/api/graph/list', (req, res, next) => {
    res.json({ graphs: graphStorage.getAll() });
});

sioServer.onMessageReceived({
    handler: (msg) => {
        switch (msg.command) {
            case '+graph':
                graphStorage.set(JSON.parse(msg.payload));
                break;

            case 'graph-data':
            case 'graph-position':
                let graphs = JSON.parse(msg.payload).graphs;
                for (let graph of graphs) {
                    graphStorage.set(graph);
                }
                break;
            case '-graph':
                graphStorage.remove(<number>msg.payload);
                break;
        }
    }
})

let markerStorage = new ObjectStorage();

webServer.addPath('/api/marker/list', (req, res, next) => {
    res.json({ markers: markerStorage.getAll() });
});

sioServer.onMessageReceived({
    handler: (msg) => {
        switch (msg.command) {
            case '+marker':
            case 'marker':
                markerStorage.set(JSON.parse(msg.payload));
                break;
            case '-marker':
                markerStorage.remove(<number>msg.payload);
                break;
        }
    }
});


let surfaceStorage: { [name: string]: any } = {};

webServer.addPath('/api/surface', (req, res, next) => {
    let params = req.body;
    res.json(surfaceStorage[params.name]);
});

sioServer.onMessageReceived({
    handler: (msg) => {
        switch (msg.command) {
            case 'surface':
                let payload = JSON.parse(msg.payload);
                surfaceStorage[payload.name] = payload;
                break;
        }
    }
});


webServer.start();
sioServer.start(webServer);
