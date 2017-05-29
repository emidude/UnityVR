using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomMsgType
{
	public const short ReadyToPlay = 100;
	public const short RestartClient = 101;
}

public class ReadyToPlayVideoMessage : MessageBase 
{
}

public class RestartClientMessage : MessageBase 
{
}
