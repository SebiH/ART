import * as http from 'http';
import * as path from 'path';
import * as express from 'express';

import { Util } from './util';

export class WebServer {

    private server: http.Server;

    public start(port: number): void {
        let app = express();
        app.set('port', port);
        app.use(express.static(path.join(__dirname, '../../client/')));

        // send clients to index page on 404
        app.use((req, res, next) => {
            res.sendFile(path.join(__dirname, '../../client/index.html'));
        })

        this.server = http.createServer(app);
        this.server.listen(port, () => {
            console.log('Web server listening on ' + Util.getIp() + ':' + port);
        });
    }

    public getServer(): http.Server {
        return this.server;
    }

    public stop(): void {
        this.server.close();
    }
}

