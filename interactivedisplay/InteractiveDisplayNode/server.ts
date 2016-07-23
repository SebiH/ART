var express = require('express');
var bodyParser = require('body-parser');
var http = require('http');
var path = require('path');

// configure & start HTTP server
var app = express();
app.set('port', process.env.PORT || 81);
app.use(express.static(path.join(__dirname + '/public/')));
app.use(bodyParser.json());

app.post('/click', function (req, res) {
    var x = req.body.x;
    var y = req.body.y;
    console.log(x + ' ' + y);

    res.end();
});

var httpServer = http.createServer(app).listen(app.get('port'), function () {
    console.log('Express server listening on port ' + app.get('port'));
});
