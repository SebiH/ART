import * as http from 'http';
import * as path from 'path';
import * as express from 'express';
import * as bodyParser from 'body-parser';

import { Util } from './util';

export class WebServer {

    private _server: http.Server;

    public Start(port: number): void {
        var app = express();
        app.set('port', port);
        app.use(express.static(path.join(__dirname, '../../client/')));
        app.use(bodyParser.json());

        app.post('/click', (req, res) => {
            // TODO: get rid of this terrible hack
            var width = 1920; // width as defined in index.html
            var height = 1080; // height as defined in index.html

            var x = req.body.x / width;
            var y = req.body.y / height;

            // convert to proper format, aka. byte array of 2 floats
            var floatBuffer = new Buffer(2 * 4);
            floatBuffer.writeFloatLE(x, 0);
            floatBuffer.writeFloatLE(y, 4);

            // sendToUnity(floatBuffer);

            res.end();
        });

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

    }
}

