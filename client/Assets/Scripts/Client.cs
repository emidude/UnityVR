using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.VR;

public class Client : MonoBehaviour {

	[SerializeField]
	private int port;

	[SerializeField]
	private VideoClip loopVideo;

	[SerializeField]
	private VideoClip experienceVideo;

	[SerializeField]
	private VideoPlayer videoPlayer;

	//DECLARED AUDIO SOURCE
	[SerializeField]
	private AudioSource audioSource;

	//DECLARED AUDIO SOURCE 2
	[SerializeField]
	public AudioSource audioSource2;

	//[Serialize Field]
	public  AudioClip audio;

	[SerializeField]
	private Canvas UI;

	[SerializeField]
	private InputField ipInput;

	[SerializeField]
	private Button connectButton;

	[SerializeField]
	private int syncEveryXFrames = 200;

	private NetworkClient client;

	private string ip;

	private void Awake()
	{
		Application.runInBackground = true;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		VRSettings.enabled = false;

		client = new NetworkClient();

		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		client.RegisterHandler(MsgType.Error, OnError);
		client.RegisterHandler(CustomMsgType.ReadyToPlay, OnReadyToPlay);
		client.RegisterHandler(CustomMsgType.RestartClient, OnRestartClient);
		client.RegisterHandler(CustomMsgType.Ping, OnPing);
		client.RegisterHandler (CustomMsgType.ResetOrientation, OnResetOrientation);

		#if UNITY_IOS
		videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
		#else 
		//Set Audio Output to AudioSource
//		audioSource = gameObject.AddComponent<AudioSource>();
//		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
//		videoPlayer.SetTargetAudioSource(0, audioSource);

		audioSource2 = gameObject.AddComponent<AudioSource>();
		audioSource2.clip = audio; 

		#endif

		
		//Assign the Audio from Video to AudioSource to be played
		//videoPlayer.EnableAudioTrack(0, true);


		connectButton.onClick.AddListener(OnConnectButtonClicked);

		PlayLoopVideo();
	}

	private void OnConnectButtonClicked ()
	{
		ip = ipInput.text;

		Debug.LogFormat("OnConnectButtonClicked");

		if(client.isConnected)
		{
			Debug.LogFormat("Disconnecting");
			client.Disconnect();
		}
		else 
		{
			client.Connect(ip, port);
			Debug.LogFormat("Trying to connect to {0}:{1}", ip, port);
		}
	
	}

	private void PlayLoopVideo ()
	{

//		//Add AudioSource
//		audioSource = gameObject.AddComponent<AudioSource>();
//		
//		//Set Audio Output to AudioSource
//		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
//		
//		//Assign the Audio from Video to AudioSource to be played
//		videoPlayer.EnableAudioTrack(0, true);
//		videoPlayer.SetTargetAudioSource(0, audioSource);

		audioSource2.Stop ();
		videoPlayer.Stop ();
		videoPlayer.clip = loopVideo;
		videoPlayer.isLooping = true;
		videoPlayer.Play();
	}

	private void PlayExperienceVideo ()
	{
		audioSource2.Stop ();
		videoPlayer.Stop ();
		videoPlayer.clip = experienceVideo;
		videoPlayer.isLooping = false;
		audioSource2.Play();
		videoPlayer.Play();

		StartCoroutine(ChangeToLoopWhenFinished());
	}

	private IEnumerator ChangeToLoopWhenFinished ()
	{
		yield return new WaitForSeconds(1.0f);
		int numFramesPassed = 0;
	
		while(videoPlayer.isPlaying)
		{
			if(++numFramesPassed % syncEveryXFrames == 0)
			{
				numFramesPassed = 0;
				if(client.isConnected)
				{
					SendVideoSyncPlaybackTime((float)videoPlayer.time);
				}
			}
			yield return new WaitForEndOfFrame();
		}

		PlayLoopVideo();
	}

	private void OnConnected (NetworkMessage netMsg)
	{
		Debug.Log(string.Format("Client has connected to server with connection id: {0}", netMsg.conn.connectionId));
		UI.gameObject.SetActive(false);
		NetworkServer.SetClientReady(netMsg.conn);
		VRSettings.enabled = true;
	}

	private void OnDisconnected (NetworkMessage netMsg)
	{
		Debug.Log("Client disconnected! Reconnecting");
		client.Connect(ip, port);
	}

	private void OnError (NetworkMessage netMsg)
	{
		Debug.Log("Client error: " + netMsg.ToString());
	}

	private void OnReadyToPlay (NetworkMessage netMsg)
	{
		ReadyToPlayVideoMessage message = netMsg.reader.ReadMessage<ReadyToPlayVideoMessage>();
		Debug.Log("on ready to play" + message);

		PlayExperienceVideo();
	}

	private void OnRestartClient (NetworkMessage netMsg)
	{
		PlayLoopVideo();

	}

	private void OnPing (NetworkMessage netMsg)
	{
		Debug.Log("ping in client received.");
		client.Send(CustomMsgType.Pong, new PongMessage());
	}

	private void OnDestroy()
	{
	}

	public void SendVideoSyncPlaybackTime(float time)
	{
		client.Send(CustomMsgType.SyncVideoPlaybackTime, new SyncVideoPlaybackTimeMessage(time));
	}


	private void OnResetOrientation(NetworkMessage netMsg)
	{
		InputTracking.Recenter ();
		Debug.Log ("recenter orientation!");
	}
}
