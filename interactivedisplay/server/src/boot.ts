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
let selectedDataIndices = null;

webServer.addPath('/api/graph/list', (req, res, next) => {
    res.json({
        graphs: graphStorage.getAll(),
        data: selectedDataIndices
    });
});

sioServer.onMessageReceived({
    handler: (msg) => {
        switch (msg.command) {
            case '+graph':
                graphStorage.set(JSON.parse(msg.payload));
                break;

            case 'graph':
                let graphs = JSON.parse(msg.payload).graphs;
                for (let graph of graphs) {
                    graphStorage.set(graph);
                }
                break;
            case '-graph':
                graphStorage.remove(<number>msg.payload);
                break;

            case 'selectedDataIndices':
                selectedDataIndices = JSON.parse(msg.payload);
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
                markerStorage.set(JSON.parse(msg.payload));
                break;

            case 'marker':
                let markers = JSON.parse(msg.payload).markers;
                for (let marker of markers) {
                    markerStorage.set(marker);
                }
                break;
            case '-marker':
                markerStorage.remove(<number>msg.payload);
                break;
            case 'marker-clear':
                markerStorage.clear();
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



let filterStorage = new ObjectStorage();

webServer.addPath('/api/filter/list', (req, res, next) => {
    res.json({ filters: filterStorage.getAll() });
});

sioServer.onMessageReceived({
    handler: (msg) => {
        switch (msg.command) {
            case '+filter':
                filterStorage.set(JSON.parse(msg.payload));
                break;

            case 'filter':
                let filters = JSON.parse(msg.payload).filters;
                for (let filter of filters) {
                    filterStorage.set(filter);
                }
                break;

            case '-filter':
                filterStorage.remove(<number>msg.payload);
        }
    }
})


let globalFilter = {};

webServer.addPath('/api/filter/global', (req, res, next) => {
    res.json({ globalFilter: globalFilter });
});

sioServer.onMessageReceived({
    handler: (msg) => {
        if (msg.command == 'globalfilter') {
            globalFilter = JSON.parse(msg.payload);
        }
    }
})

webServer.start();
sioServer.start(webServer);
