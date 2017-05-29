using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Assertions;

public class Server : MonoBehaviour {

	public Action OnClientConnected;
	public Action OnClientDisconnected;

	[SerializeField]
	private int port;

	private NetworkServerSimple server;

	private NetworkConnection clientConnection;

	private void Awake()
	{
		Debug.Log("initialize server to listen on : " + port);
		server = new NetworkServerSimple();
		server.Listen(port);

		server.RegisterHandler(MsgType.Connect, OnServerConnect);
		server.RegisterHandler(MsgType.Disconnect, OnServerDisonnect);
	
	}
		
	private void OnServerConnect(NetworkMessage netMsg)
	{
		if(clientConnection != null)
		{
			Debug.LogWarning("there's already one client connected, ignoring");
			return;

		}
		Debug.Log(string.Format("Server: Client has connected to server with connection id: {0}", netMsg.conn.connectionId));

		clientConnection = netMsg.conn;

		if(OnClientConnected != null)
		{
			OnClientConnected();
		}
	}

	private void OnServerDisonnect (NetworkMessage netMsg)
	{
		Debug.Log("disconnect!");

		if(OnClientDisconnected != null)
		{
			OnClientDisconnected();
		}

		Assert.AreEqual(netMsg.conn, clientConnection);
		clientConnection = null;
	}

	private void Update()
	{
		if(server != null)
		{
			server.Update();
		}
	}

	private void OnDestroy()
	{
		if(server != null)
		{
			server.Stop();
		}
	}

	public void SendPlayVideo()
	{
		clientConnection.Send(CustomMsgType.ReadyToPlay, new ReadyToPlayVideoMessage());
	}

	public void SendReset ()
	{
		clientConnection.Send(CustomMsgType.RestartClient, new RestartClientMessage());
	}
}
