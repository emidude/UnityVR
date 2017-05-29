using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Video;

public class Client : MonoBehaviour {

	[SerializeField]
	private string ip;

	[SerializeField]
	private int port;

	[SerializeField]
	private VideoPlayer videoPlayer;

	private NetworkClient client;

	private void Awake()
	{

		client = new NetworkClient();

		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		client.RegisterHandler(MsgType.Error, OnError);
		client.RegisterHandler(CustomMsgType.ReadyToPlay, OnReadyToPlay);
		client.RegisterHandler(CustomMsgType.RestartClient, OnRestartClient);

		client.Connect(ip, port);
		Debug.LogFormat("Trying to connect to {0}:{1}", ip, port);
	
	}

	private void OnConnected (NetworkMessage netMsg)
	{
		Debug.Log(string.Format("Client has connected to server with connection id: {0}", netMsg.conn.connectionId));
		NetworkServer.SetClientReady(netMsg.conn);
	}

	private void OnDisconnected (NetworkMessage netMsg)
	{
		Debug.Log("Client disconnected!");
	}

	private void OnError (NetworkMessage netMsg)
	{
		Debug.Log("Client error: " + netMsg.ToString());
	}

	private void OnServerDisonnect (NetworkMessage netMsg)
	{
		Debug.Log("disconnect!");

	}

	private void OnReadyToPlay (NetworkMessage netMsg)
	{
		ReadyToPlayVideoMessage message = netMsg.reader.ReadMessage<ReadyToPlayVideoMessage>();
		Debug.Log("on ready to play" + message);

		videoPlayer.Play();
	}

	private void OnRestartClient (NetworkMessage netMsg)
	{
		videoPlayer.Stop();
	}

	private void OnDestroy()
	{
	}
}
