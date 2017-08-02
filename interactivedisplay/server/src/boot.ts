import { UnityServer } from "./unity-server";
import { UnityMessageListener } from './unity-message-listener';
import { WebServer } from "./web-server";
import { SocketIOServer } from "./socketio-server";
import { SocketIOMessageListener } from './socketio-message-listener';
import { GraphDataProvider } from './graph-data-provider';
import { ObjectStorage } from './object-storage';
import { SqlConnection } from './sql-connection';
import * as Colors from './colors';
import * as fs from 'fs';
import * as _ from 'lodash';

const UNITY_PORT = 8835;
const WEB_PORT = 81; // 80 might already be in use, thanks skype!

let config = require('../sql.conf.json');


//
// Save/Load entire state
//

const cachePath = './data/cache.json';
let globalState: any = {};

let saveGlobalState = _.throttle(() => {
    fs.writeFile(cachePath, JSON.stringify(globalState, null, '  '), (err) => {
        if (err) console.error('Could not save global state: ' + err.message);
    });
}, 1000, { leading: false });



if (fs.existsSync(cachePath)) {
    fs.readFile(cachePath, (err, data) => {
        if (err) throw err;
        globalState = JSON.parse(data.toString());

        for (let graph of globalState.graphs) {
            graphStorage.set(graph);
        }
        for (let filter of globalState.filters) {
            filterStorage.set(filter);
        }

        console.log('Loaded global state from cache');
    });
} else {
    console.log('Starting with empty cache!');
}



let unityServer = new UnityServer();
unityServer.start(UNITY_PORT);


let webServer = new WebServer(WEB_PORT);

let graphDataProvider = new GraphDataProvider(config);
webServer.addPath('/api/graph/data', (req, res, next) => {
    let params = req.body;

    graphDataProvider.getData(params['dimension'], (data) => {
        res.json(data);
    });
});


webServer.addPath('/api/setquery', (req, res, next) => {
    let params = req.body;

    let source = graphDataProvider.getDataSource();

    if (source instanceof SqlConnection) {
        source.setSqlQuery(params.sql);
    }
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
                    if (graphStorage.has(graph)) {
                        graphStorage.set(graph);
                    }
                }
                break;
            case '-graph':
                graphStorage.remove(<number>msg.payload);
                break;

            case 'selectedDataIndices':
                selectedDataIndices = JSON.parse(msg.payload);
                break;
        }

        globalState.graphs = graphStorage.getAll();
        saveGlobalState();
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

        globalState.filters = filterStorage.getAll();
        saveGlobalState();
    }
});


webServer.addPath('/api/filter/global', (req, res, next) => {
    res.json({ globalfilter: globalState.globalFilter });
});

sioServer.onMessageReceived({
    handler: (msg) => {
        if (msg.command == 'globalfilter') {
            globalState.globalFilter = JSON.parse(msg.payload).globalfilter;
            saveGlobalState();
        }
    }
});


console.log("Using config: " + JSON.stringify(config.settings, null, '    '));
let clientSettings = config.settings;

webServer.addPath('/api/settings', (req, res, next) => {
    res.json(clientSettings);
});

sioServer.onMessageReceived({
    handler: (msg) => {
        if (msg.command == 'settings') {
            clientSettings = JSON.parse(msg.payload);
            sioServer.broadcast(msg.command, msg.payload, msg.origin);
        }
        if (msg.command == 'renew-graphs') {
            sioServer.broadcast(msg.command, msg.payload, msg.origin);
        }
    }
});




webServer.start();
sioServer.start(webServer);
