import * as http from 'http';
import * as path from 'path';
import * as express from 'express';
import * as bodyparser from 'body-parser';

import { Util } from './util';

export class WebServer {

    private app: express.Application;
    private server: http.Server;

    public start(port: number): void {
        this.app = express();
        this.app.set('port', port);

        // handle POST data
        this.app.use(bodyparser.urlencoded({ extended: false }));
        this.app.use(bodyparser.json());

        // set up default routes
        this.app.use(express.static(path.join(__dirname, '../../client/')));

        // start server
        this.server = http.createServer(this.app);
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

    public addPath(path: string, requestHandler: express.RequestHandler) {
        this.app.use(path, requestHandler);
    }
}

