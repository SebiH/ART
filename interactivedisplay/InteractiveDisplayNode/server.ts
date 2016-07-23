var express = require('express');
var http = require('http');
var path = require('path');

// configure & start HTTP server
var app = express();
app.set('port', process.env.PORT || 80);
app.use(express.static(path.join(__dirname + '/public/')));

var httpServer = http.createServer(app).listen(app.get('port'), function () {
    console.log('Express server listening on port ' + app.get('port'));
});
