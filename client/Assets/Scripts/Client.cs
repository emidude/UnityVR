using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Video;
using UnityEngine.UI;

public class Client : MonoBehaviour {

	[SerializeField]
	private string ip;

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

	private NetworkClient client;

	private void Awake()
	{
		client = new NetworkClient();

		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		client.RegisterHandler(MsgType.Error, OnError);
		client.RegisterHandler(CustomMsgType.ReadyToPlay, OnReadyToPlay);
		client.RegisterHandler(CustomMsgType.RestartClient, OnRestartClient);

		connectButton.onClick.AddListener(OnConnectButtonClicked);

		PlayLoopVideo();
	}

	private void OnConnectButtonClicked ()
	{
		client.Connect(ipInput.text, port);
		Debug.LogFormat("Trying to connect to {0}:{1}", ip, port);
	}

	private void PlayLoopVideo ()
	{
		videoPlayer.clip = loopVideo;
		videoPlayer.isLooping = true;
		videoPlayer.Play();
	}

	private void PlayExperienceVideo ()
	{
		videoPlayer.clip = experienceVideo;
		videoPlayer.isLooping = false;
		videoPlayer.Play();

		StartCoroutine(ChangeToLoopWhenFinished());
	}

	private IEnumerator ChangeToLoopWhenFinished ()
	{
		while(videoPlayer.isPlaying)
		{
			yield return new WaitForEndOfFrame();
		}

		PlayLoopVideo();
	}

	private void OnConnected (NetworkMessage netMsg)
	{
		UI.gameObject.SetActive(false);
		Debug.Log(string.Format("Client has connected to server with connection id: {0}", netMsg.conn.connectionId));
		NetworkServer.SetClientReady(netMsg.conn);
	}

	private void OnDisconnected (NetworkMessage netMsg)
	{
		UI.gameObject.SetActive(true);
		Debug.Log("Client disconnected!");
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

	private void OnDestroy()
	{
	}
}
