using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomMsgType
{
	public const short ReadyToPlay = 100;
	public const short RestartClient = 101;
	public const short Ping = 102;
	public const short Pong = 103;
	public const short SyncVideoPlaybackTime = 104;
}

public class ReadyToPlayVideoMessage : MessageBase 
{
}

public class RestartClientMessage : MessageBase 
{
}

public class PingMessage : MessageBase 
{
}

public class PongMessage : MessageBase 
{
}

public class SyncVideoPlaybackTimeMessage : MessageBase 
{
	public readonly float Time;

	public SyncVideoPlaybackTimeMessage()
	{}

	public SyncVideoPlaybackTimeMessage(float time)
	{
		this.Time = time;
	}
}