// configure TCP server for unity client
// adapted from https://gist.github.com/creationix/707146
var net = require('net');
var clients = [];

function sendToUnity(msg) {

    console.log("Sending " + msg.readFloatLE(0) + ', ' + msg.readFloatLE(4) + " to all clients");
    for (var client of clients)
    {
        try {
            client.write(msg);
        } catch (e) {
            console.log(e);
        }
    }
}

net.createServer(function (socket) {

    socket.name = socket.remoteAddress + ":" + socket.remotePort
    console.log("Client " + socket.name + " connected");

    clients.push(socket);

    // Remove the client from the list when it leaves
    socket.on('end', () => {
        clients.splice(clients.indexOf(socket), 1);
        console.log("Client " + socket.name + " disconnected");
    });
    socket.on("error", (err) => {
        console.log(err.stack);
        clients.splice(clients.indexOf(socket), 1);
        console.log("Client " + socket.name + " disconnected with error");
    });


}).listen(8835);



// configure & start HTTP server
var express = require('express');
var bodyParser = require('body-parser');
var http = require('http');
var path = require('path');


var app = express();
app.set('port', process.env.PORT || 81);
app.use(express.static(path.join(__dirname + '/../')));
app.use(bodyParser.json());

app.post('/click', function (req, res) {
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

app.use(function(req, res, next) {
    res.sendFile(path.join(__dirname + '/../index.html'));
})

var httpServer = http.createServer(app).listen(app.get('port'), function () {
    console.log('Express server listening on port ' + app.get('port'));
});
