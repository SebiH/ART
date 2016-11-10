import * as http from 'http';
import * as path from 'path';
import * as express from 'express';

import { Util } from './util';

export class WebServer {

    private _server: http.Server;

    public Start(port: number): void {
        let app = express();
        app.set('port', port);
        app.use(express.static(path.join(__dirname, '../../client/')));

        // send clients to index page on 404
        app.use((req, res, next) => {
            res.sendFile(path.join(__dirname, '../../client/index.html'));
        })

        this._server = http.createServer(app);
        this._server.listen(port, () => {
            console.log('Web server listening on ' + Util.GetIp() + ':' + port);
        });
    }

    public GetServer(): any {
        return this._server;
    }

    public Stop(): void {
        this._server.close();
    }
}

