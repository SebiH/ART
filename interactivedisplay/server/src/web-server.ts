import * as http from 'http';
import * as path from 'path';
import * as express from 'express';
import * as bodyparser from 'body-parser';

import { Util } from './util';

export class WebServer {

    private isRunning: boolean = false;
    private port: number = 80;
    private app: express.Application;
    private server: http.Server;

    public constructor(port: number) {
        this.app = express();
        this.app.set('port', port);
        this.port = port;

        // handle POST data
        this.app.use(bodyparser.urlencoded({ extended: false }));
        this.app.use(bodyparser.json());

        // set up default routes
        this.app.use(express.static(path.join(__dirname, '../../client/')));
    }

    public start(): void {
        // add default route for 404s
        this.app.use((req, res, next) => {
            res.sendFile(path.join(__dirname, '../../client/index.html'));
        });

        // lock server so no more route changes are allowed etc
        this.isRunning = true;

        // start server
        this.server = http.createServer(this.app);
        this.server.listen(this.port, () => {
            console.log('Web server listening on ' + Util.getIp() + ':' + this.port);
        });
    }

    public getServer(): http.Server {
        return this.server;
    }

    public stop(): void {
        this.server.close();
        this.isRunning = false;
    }

    public addPath(path: string, requestHandler: express.RequestHandler) {
        if (this.isRunning) {
            console.error("Could not add route " + path + ": Server already running");
        } else {
            this.app.use(path, requestHandler);
        }
    }
}
