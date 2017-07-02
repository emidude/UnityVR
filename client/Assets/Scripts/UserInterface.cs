using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UserInterface : MonoBehaviour {

	[SerializeField]
	private Button playVideoButton;

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Text waitingForClientText;

	[SerializeField]
	private Text ipText;

	[SerializeField]
	private Text delayText;

	[SerializeField]
	private Server server;

	[SerializeField]
	private VideoPlayer videoPlayer;

	private void Awake () 
	{
		server.OnClientConnected += OnClientConnected;
		server.OnClientDisconnected += OnClientDisconnected;
		playVideoButton.onClick.AddListener(OnPlayButtonClicked);
		resetButton.onClick.AddListener(OnResetButtonClicked);
		resetButton.gameObject.SetActive(false);
		playVideoButton.gameObject.SetActive(false);

		ipText.text =  string.Format("Your ip is: {0}", Network.player.ipAddress);
	}

	private void Update()
	{
		delayText.text = Mathf.RoundToInt(server.Delay * 1000) + " ms" + "Latency: " + server.Latency;
	}

	private void OnClientConnected ()
	{
		waitingForClientText.gameObject.SetActive(false);
		playVideoButton.gameObject.SetActive(true);

	}

	private void OnClientDisconnected ()
	{
		waitingForClientText.gameObject.SetActive(true);
		playVideoButton.gameObject.SetActive(false);
	}

	private void OnPlayButtonClicked ()
	{
		server.SendPlayVideo();
		videoPlayer.Play();
		resetButton.gameObject.SetActive(true);
	}
		
	private void OnResetButtonClicked ()
	{
		server.SendReset();
		videoPlayer.Stop();


		playVideoButton.gameObject.SetActive(true);
		resetButton.gameObject.SetActive(false);
	}
}
