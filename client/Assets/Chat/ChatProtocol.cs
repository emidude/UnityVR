

using UnityEngine.Networking;

public class ChatMsg
{
	public const short ChannelJoin = 100;
	public const short ChannelLeave = 101;
	public const short Talk = 102;
	public const short Login = 103;
	public const short ChannelCreate = 104;
	public const short ListChannels = 105;
}

public class ChannelJoinRequestMessage : MessageBase
{
	public ChatPersonId personId;
	public string name;
}

public class ChannelJoinResponseMessage : MessageBase
{
	public ChatPersonId personId;
	public ChatChannelId channelId;
	public string channelName;
	public string personName;
}

public class ChannelLeaveMessage : MessageBase
{
	public ChatPersonId personId;
	public ChatChannelId channelId;
}

public class ChannelLeaveResponseMessage : MessageBase
{
	public ChatPersonId personId;
	public ChatChannelId channelId;
}

public class ChannelCreateMessage : MessageBase
{
	public ChatPersonId personId;
	public string channelName;
}

public class ChannelCreateResponseMessage : MessageBase
{
	public ChatPersonId personId;
	public string channelName;
	public ChatChannelId channelId;
}


public class TalkMessage : MessageBase
{
	public ChatPersonId personId;
	public ChatChannelId channelId;
	public string text;
}

public class LoginMessage : MessageBase
{
	public string personName;
}

public class LoginResponseMessage : MessageBase
{
	public string personName;
	public ChatPersonId personId;
}

public struct ChannelInfo
{
	public ChatChannelId channelId;
	public string channelName;
}

public class ListChannelsResponseMessage : MessageBase
{
	public ChannelInfo[] channels;
}

