import * as net from 'net';
import * as _ from 'lodash';

var clients = [];

net.createServer((socket) => {
    socket.write('Hello, unity!');

    socket.on('data', (data) => {
        console.log("" + data);
    });

    socket.on('error', (err) => {
        console.log("" + err);
    });

    socket.on('end', () => {

    });

}).listen(8835);

console.log('TCP Server listening on 8835');


function sendToUnity(msg) {
}

// configure & start HTTP server
import * as http from 'http';
import * as path from 'path';
import * as express from 'express';
import * as bodyParser from 'body-parser';

var app = express();
app.set('port', process.env.PORT || 81);
app.use(express.static(path.join(__dirname + '/../client/')));
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

    sendToUnity(floatBuffer);

    res.end();
});

app.use((req, res, next) => {
    res.sendFile(path.join(__dirname + '/../client/index.html'));
})

var httpServer = http.createServer(app).listen(app.get('port'), () => {
    console.log('Express server listening on port ' + app.get('port'));
});
