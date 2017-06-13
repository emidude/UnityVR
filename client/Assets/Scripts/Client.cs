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

	[SerializeField]
	private Canvas UI;

	[SerializeField]
	private InputField ipInput;

	[SerializeField]
	private Button connectButton;

	[SerializeField]
	private int syncEveryXFrames = 60;

	private NetworkClient client;

	private string ip;

	private void Awake()
	{
		VRSettings.enabled = false;

		client = new NetworkClient();

		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		client.RegisterHandler(MsgType.Error, OnError);
		client.RegisterHandler(CustomMsgType.ReadyToPlay, OnReadyToPlay);
		client.RegisterHandler(CustomMsgType.RestartClient, OnRestartClient);
		client.RegisterHandler(CustomMsgType.Ping, OnPing);


		connectButton.onClick.AddListener(OnConnectButtonClicked);

		PlayLoopVideo();
	}

	private void OnConnectButtonClicked ()
	{
		ip = ipInput.text;
		client.Connect(ip, port);
		Debug.LogFormat("Trying to connect to {0}:{1}", ip, port);
	}

	private void PlayLoopVideo ()
	{
		videoPlayer.Stop ();
		videoPlayer.clip = loopVideo;
		videoPlayer.isLooping = true;
		videoPlayer.Play();
	}

	private void PlayExperienceVideo ()
	{
		videoPlayer.Stop ();
		videoPlayer.clip = experienceVideo;
		videoPlayer.isLooping = false;
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
				SendVideoSyncPlaybackTime((float)videoPlayer.time);
			}
			yield return new WaitForEndOfFrame();
		}

		PlayLoopVideo();
	}

	private void OnConnected (NetworkMessage netMsg)
	{
		UI.gameObject.SetActive(false);
		Debug.Log(string.Format("Client has connected to server with connection id: {0}", netMsg.conn.connectionId));
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

	private void OnServerDisonnect (NetworkMessage netMsg)
	{
		UI.gameObject.SetActive(true);
		Debug.Log("disconnect!");

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
}
