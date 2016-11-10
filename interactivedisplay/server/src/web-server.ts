import * as http from 'http';
import * as path from 'path';
import * as express from 'express';
import * as bodyParser from 'body-parser';

export class WebServer {

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

        app.use((req, res, next) => {
            res.sendFile(path.join(__dirname, '../../client/index.html'));
        })

        var httpServer = http.createServer(app).listen(port, () => {
            console.log('Express server listening on port ' + port);
        });
    }

    public Stop(): void {

    }

    public Broadcast(msg: string): void {

    }

    public OnMessageReceived(): void {

    }
}

