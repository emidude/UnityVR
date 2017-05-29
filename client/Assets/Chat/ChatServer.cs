using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;


public class ChatServer : MonoBehaviour
{
	NetworkServerSimple server = null;

	public int chatServerPort = 9999;
	public int maxConnections = 48;

	public Dictionary<ChatChannelId, ChatChannel> channels = new Dictionary<ChatChannelId, ChatChannel>();
	Dictionary<string, ChatChannel> channelsByName = new Dictionary<string, ChatChannel>();

	public Dictionary<ChatPersonId, ChatPerson> people = new Dictionary<ChatPersonId, ChatPerson>();
	public Dictionary<int, ChatPerson> logins = new Dictionary<int, ChatPerson>();


	void Setup()
	{
		server = new NetworkServerSimple();
		server.RegisterHandler(MsgType.Connect, OnConnect);
		server.RegisterHandler(MsgType.Disconnect, OnDisconnect);
		server.RegisterHandler(ChatMsg.ChannelJoin, OnChannelJoin);
		server.RegisterHandler(ChatMsg.ChannelLeave, OnChannelLeave);
		server.RegisterHandler(ChatMsg.ChannelCreate, OnChannelCreate);
		server.RegisterHandler(ChatMsg.Talk, OnTalk);
		server.RegisterHandler(ChatMsg.Login, OnLogin);
		server.RegisterHandler(ChatMsg.ListChannels, OListChannels);

		server.Listen(chatServerPort);
	}

	void Update()
	{
		if (server != null)
		{
			server.Update();
		}
	}

	void OnGUI()
	{
		if (server == null)
		{
			if (GUI.Button(new Rect(110, 20, 200, 20), "Start Chat Server (s)") || Input.GetKeyDown(KeyCode.S))
			{
				Setup();
			}
		}
		else
		{
			if (GUI.Button(new Rect(110, 20, 200, 20), "Stop Chat Server (s)") || Input.GetKeyDown(KeyCode.S))
			{
				server.Stop();
				server = null;
			}
		}
	}

	// ------------------------------ utilities ----------------------------

	ChatChannel FindChannel(string name)
	{
		if (channelsByName.ContainsKey(name))
		{
			return channelsByName[name];
		}
		return null;
	}

	ChatChannel FindChannel(ChatChannelId id)
	{
		if (channels.ContainsKey(id))
		{
			return channels[id];
		}
		return null;
	}

	ChatPerson FindPerson(ChatPersonId id)
	{
		if (people.ContainsKey(id))
		{
			return people[id];
		}
		return null;
	}

	// ------------------------------ msg handers ----------------------------

	void OnConnect(NetworkMessage netMsg)
	{
		Debug.Log("Chat client connect");
	}

	void OnDisconnect(NetworkMessage netMsg)
	{
		Debug.Log("Chat client disconnect");
	}


	void OnLogin(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<LoginMessage>();

		if (logins.ContainsKey(netMsg.conn.connectionId))
		{
			Debug.LogError("Login: already logged in");
			return;
		}

		var person = new ChatPerson(msg.personName, netMsg.conn);

		logins[netMsg.conn.connectionId] = person;
		people[person.chatPersonId] = person;

		Debug.Log("Login: " + person.personName + " " + person.chatPersonId);

		var response = new LoginResponseMessage();
		response.personName = msg.personName;
		response.personId = person.chatPersonId;
		netMsg.conn.Send(ChatMsg.Login, response);
	}

	void OListChannels(NetworkMessage netMsg)
	{
		var response = new ListChannelsResponseMessage();

		List<ChannelInfo> responseChannels = new List<ChannelInfo>();
		foreach (var c in channels.Values)
		{
			var info = new ChannelInfo();
			info.channelId = c.chatChannelId;
			info.channelName = c.channelName;
			responseChannels.Add(info);
		}
		response.channels = responseChannels.ToArray();
		netMsg.conn.Send(ChatMsg.ListChannels, response);
	}

	void OnChannelCreate(NetworkMessage netMsg)
	{
		if (!logins.ContainsKey(netMsg.conn.connectionId))
		{
			Debug.LogError("Not logged in");
			return;
		}

		var msg = netMsg.ReadMessage<ChannelCreateMessage>();

		var person = FindPerson(msg.personId);
		if (person == null)
		{
			Debug.LogError("person not found " + msg.personId);
			return;
		}

		foreach (var ch in channels.Values)
		{
			if (ch.channelName == msg.channelName)
			{
				Debug.Log("Create: already exists " + msg.channelName);
				return;
			}
		}

		var channel = new ChatChannel(msg.channelName, person);
		channels[channel.chatChannelId] = channel;
		channelsByName[channel.channelName] = channel;
		channel.ServerCreate(person);

		Debug.Log("Create " + channel.channelName + " for:" + person.chatPersonId);

		var response = new ChannelCreateResponseMessage();
		response.personId = person.chatPersonId;
		response.channelName = channel.channelName;
		response.channelId = channel.chatChannelId;

		netMsg.conn.Send(ChatMsg.ChannelCreate, response);

	}

	void OnChannelJoin(NetworkMessage netMsg)
	{
		if (!logins.ContainsKey(netMsg.conn.connectionId))
		{
			Debug.LogError("Not logged in");
			return;
		}

		var msg = netMsg.ReadMessage<ChannelJoinRequestMessage>();

		var channel = FindChannel(msg.name);
		if (channel == null)
		{
			Debug.LogError("channel not found " + msg.name);
			return;
		}

		var person = FindPerson(msg.personId);
		if (person == null)
		{
			Debug.LogError("person not found " + msg.personId);
			return;
		}

		channel.ServerJoin(person);
		Debug.Log("Join: " + channel.channelName + " " + person.chatPersonId);

	}

	void OnChannelLeave(NetworkMessage netMsg)
	{
		if (!logins.ContainsKey(netMsg.conn.connectionId))
		{
			Debug.LogError("Not logged in");
			return;
		}

		var msg = netMsg.ReadMessage<ChannelLeaveMessage>();

		var channel = FindChannel(msg.channelId);
		if (channel == null)
			return;

		var person = FindPerson(msg.personId);
		if (person == null)
			return;

		channel.ServerLeave(person);
		Debug.Log("Leave: " + channel.channelName + " " + person.chatPersonId);
	}

	void OnTalk(NetworkMessage netMsg)
	{
		if (!logins.ContainsKey(netMsg.conn.connectionId))
		{
			Debug.LogError("OnTalk Not logged in");
			return;
		}

		var msg = netMsg.ReadMessage<TalkMessage>();

		var channel = FindChannel(msg.channelId);
		if (channel == null)
		{
			Debug.LogError("OnTalk channel not found " + msg.channelId);
			return;
		}

		var person = FindPerson(msg.personId);
		if (person == null)
		{
			Debug.LogError("OnTalk person not found " + msg.personId);
			return;
		}

		channel.ServerSay(person, msg.text);
		Debug.Log("Talk: " + msg.text + " " + person.chatPersonId);
	}

}

