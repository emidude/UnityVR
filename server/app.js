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

	socket.on("USER_CONNECT", function() 
	{
		console.log("user connected");

		for(var i = 0; i < clients.length; i++)
		{
			socket.emit("USER_CONNECTED", {name: clients[i].name, position: clients[i].position});
			console.log("User name " + clients[i].name + " is connected");
		}
	});

	socket.on("beep", function()
	{
		console.log("received beep");

		socket.emit("boop");
	});
});


