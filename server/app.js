var express = require('express');
var socketio = require('socket.io');
var http = require('http');

var app = express()
var server = http.createServer(app);
var io = socketio.listen(server);

app.set('port', process.env.PORT || 3001);

var clients = [];


server.listen(app.get('port'), 
	
	function()
	{
		console.log("server is running");
	});

io.on("connection", function(socket)
{
	console.log("someone connected");

	var currentUser;

	socket.on("register", function(client)
	{
		console.log("register " + client.userId)
		clients.push(client.userId);

		if(clients.length == 2)
		{
			socket.emit("playVideo");
		}
	});
});


