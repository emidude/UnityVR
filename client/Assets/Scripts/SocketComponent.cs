using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class SocketComponent : MonoBehaviour {

	[SerializeField]
	private string socketServerUrl;

	private Socket socket ;

	private void Awake()
	{
		socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
