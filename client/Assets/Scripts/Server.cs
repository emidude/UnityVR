using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Assertions;
using UnityEngine.VR;

public class Server : MonoBehaviour {

	public Action OnClientConnected;
	public Action OnClientDisconnected;

	[SerializeField]
	private int port;

	private NetworkServerSimple server;

	private NetworkConnection clientConnection;

	private float timePingSent;
	private bool waitingForPingResponse = false;
	private List<float> pingTimes = new List<float>();

	private float latency = 0f;


	private void Awake()
	{
		VRSettings.enabled = false;

		Debug.Log("initialize server to listen on : " + port);
		server = new NetworkServerSimple();
		server.Listen(port);

		server.RegisterHandler(MsgType.Connect, OnServerConnect);
		server.RegisterHandler(MsgType.Disconnect, OnServerDisonnect);
		server.RegisterHandler(CustomMsgType.Ping, OnPingResponse);
	
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

		StartCoroutine(DetermineLatency());

		if(OnClientConnected != null)
		{
			OnClientConnected();
		}
	}

	private IEnumerator DetermineLatency ()
	{
		int numSamples = 5;

		while(numSamples > 0)
		{
			if(waitingForPingResponse)
			{
				yield return new WaitForEndOfFrame();
			}
			else 
			{
				numSamples--;
				timePingSent = Time.time;
				clientConnection.Send(CustomMsgType.Ping, new PingMessage());
			}
		}

		for(int i = 0; i < pingTimes.Count; i++)
		{
			latency += pingTimes[i];
		}

		latency /= pingTimes.Count;

		Debug.Log("latency: " + latency*1000);
	}

	private void OnPingResponse (NetworkMessage netMsg)
	{
		waitingForPingResponse = false;
		float pingTime = Time.time - timePingSent;
		Debug.Log("Ping " + pingTime*1000);
		pingTimes.Add(pingTime);
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
