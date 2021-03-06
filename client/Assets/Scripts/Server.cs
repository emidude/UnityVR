﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Assertions;
using UnityEngine.VR;
using UnityEngine.Video;

public class Server : MonoBehaviour {

	public Action OnClientConnected;
	public Action OnClientDisconnected;

	[SerializeField]
	private float acceptableDelay = 0.1f;

	[SerializeField]
	private float videoSpeedUpMultiplier = 1.1f;

	[SerializeField]
	private int port;

	[SerializeField]
	private VideoPlayer videoPlayer2;

	[SerializeField]
	private VideoClip loopVideo2;

	[SerializeField]
	private VideoClip experienceVideo2;


	//DECLARED AUDIO SOURCE
	[SerializeField]
	private AudioSource audioSource;

	private NetworkServerSimple server;

	private NetworkConnection clientConnection;

	private float timePingSent;
	private bool waitingForPingResponse = false;
	private bool latencySequenceFinished = false;
	private List<float> pingTimes = new List<float>();

	private bool isPlayingExperienceVideo2 = false;

	public  float Latency
	{
		get; private set;
	}

	public  float Delay
	{
		get; private set;
	}

	private void Awake()
	{
		Application.runInBackground = true;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		Delay = 0f;

		VRSettings.enabled = false;

		Debug.Log("initialize server to listen on : " + port);
		server = new NetworkServerSimple();
		server.Listen(port);

		server.RegisterHandler(MsgType.Connect, OnServerConnect);
		server.RegisterHandler(MsgType.Disconnect, OnServerDisonnect);
		server.RegisterHandler(CustomMsgType.Pong, OnPongResponse);
		server.RegisterHandler(CustomMsgType.SyncVideoPlaybackTime, OnSyncVideoPlaybackTime);

		#if UNITY_IOS
		videoPlayer2.audioOutputMode = VideoAudioOutputMode.Direct;
		#else 
		//Set Audio Output to AudioSource
		audioSource = gameObject.AddComponent<AudioSource>();
		videoPlayer2.audioOutputMode = VideoAudioOutputMode.AudioSource;
		videoPlayer2.SetTargetAudioSource(0, audioSource);
		#endif


		//Assign the Audio from Video to AudioSource to be played
		videoPlayer2.EnableAudioTrack(0, true);
		PlayLoopVideo2();
	}
		
	private void OnServerConnect(NetworkMessage netMsg)
	{
		Debug.Log(string.Format("Client trying to connect connection id: {0}", netMsg.conn.connectionId));

		if(clientConnection != null)
		{
			Debug.LogWarning("there's already one client connected, ignoring");
			return;

		}
		Debug.Log(string.Format("Server: Client has connected to server with connection id: {0}", netMsg.conn.connectionId));

		clientConnection = netMsg.conn;

		if(latencySequenceFinished && OnClientConnected != null)
		{
			OnClientConnected();
		}

		StartCoroutine(DetermineLatency());
	}

	private IEnumerator DetermineLatency ()
	{
		yield return new WaitForSeconds (0.1f);
		latencySequenceFinished = false;
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

				/////////////////////////////////////
				/// Debu
				Debug.Log("sending ping message");
				timePingSent = Time.realtimeSinceStartup;
				clientConnection.Send(CustomMsgType.Ping, new PingMessage());
				waitingForPingResponse = true;
				StartCoroutine(RetryPingAfterTimeout());
				yield return new WaitForEndOfFrame();
			}
		}

		latencySequenceFinished = true;
	}

	private IEnumerator RetryPingAfterTimeout ()
	{
		yield return new WaitForSeconds(1.0f);
		timePingSent = Time.realtimeSinceStartup;
		clientConnection.Send(CustomMsgType.Ping, new PingMessage());
	}

	private void OnPongResponse (NetworkMessage netMsg)
	{
		StopCoroutine("RetryPingAfterTimeout");
		waitingForPingResponse = false;
		//////////////////////////////////////////
		float pingTime = (Time.realtimeSinceStartup - timePingSent)/2;
		Debug.Log("Ping " + pingTime*1000);
		pingTimes.Add(pingTime);

		if(latencySequenceFinished)
		{
			Latency = 0f;

			for(int i = 0; i < pingTimes.Count; i++)
			{
				Latency += pingTimes[i];
			}

			Latency /= pingTimes.Count;

			Debug.Log("latency: " + Latency*1000);

			if(OnClientConnected != null)
			{
				OnClientConnected();
			}
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
		videoPlayer2.Stop ();
		videoPlayer2.clip = experienceVideo2;
		isPlayingExperienceVideo2 = true;
		videoPlayer2.isLooping = false;
		clientConnection.Send(CustomMsgType.ReadyToPlay, new ReadyToPlayVideoMessage());

	}

	public void SendReset ()
	{
		clientConnection.Send(CustomMsgType.RestartClient, new RestartClientMessage());
		isPlayingExperienceVideo2 = false;
		PlayLoopVideo2 ();
	}

	private void OnSyncVideoPlaybackTime (NetworkMessage netMsg)
	{
		if (!isPlayingExperienceVideo2 || !videoPlayer2.isPlaying) 
		{
			return;
		}

		if(clientConnection == null)
		{
			clientConnection = netMsg.conn;
		}

		SyncVideoPlaybackTimeMessage message = netMsg.reader.ReadMessage<SyncVideoPlaybackTimeMessage>();
	//	StartCoroutine(DetermineLatency());
		//Delay = (float)videoPlayer.time - (message.Time);
		Delay = (float)videoPlayer2.time - (message.Time + Latency);

		videoPlayer2.playbackSpeed = 1.0f;

		Debug.Log("delay between server and client is " + Delay);

		if(Delay > acceptableDelay && videoPlayer2.isPlaying)
		{
			StartCoroutine(PauseToSync(Delay));
		}

		if(Delay < -acceptableDelay)
		{
			float speed = 1.0f + Mathf.Abs (Delay) * videoSpeedUpMultiplier;
			Debug.Log("speeding up the video to catch up to client. Speed = " + speed);
			videoPlayer2.playbackSpeed = speed;
		}
	}

	private IEnumerator PauseToSync (float delay)
	{
		Debug.Log("pause for " + delay);
		videoPlayer2.Pause();
		yield return new WaitForSeconds(delay);
		videoPlayer2.Play();
		Debug.Log("resume");
	}

	public void SendRestartOrientationMessage()
	{
		clientConnection.Send (CustomMsgType.ResetOrientation, new ResetOrientationMessage ());
	}

	private void PlayLoopVideo2 ()
	{
		
		videoPlayer2.Stop ();
		videoPlayer2.clip = loopVideo2;
	//	videoPlayer.Prepare ();
		videoPlayer2.isLooping = true;
		videoPlayer2.Play();
	}

}
