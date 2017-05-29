using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChatClient : MonoBehaviour
{
	public int xoffset = 0;

	public string chatServerHost = "127.0.0.1";
	public int chatServerPort = 9999;
	public string chatName = "someDude";
	public ChatPerson myChatPerson;
	
	public Dictionary<ChatChannelId, ChatChannel> channels = new Dictionary<ChatChannelId, ChatChannel>();
	public Dictionary<ChatPersonId, ChatPerson> people = new Dictionary<ChatPersonId, ChatPerson>();

	public ChannelInfo[] channelList = null;

	NetworkClient client = null;

	string createChannelName = "default";
	string joinChannelName = "default";
	string talkText = "Hello!";

	const int kMaxChannelMessages = 10;

	void Setup()
	{
		client = new NetworkClient();
		client.RegisterHandler(MsgType.Connect, OnConnect);
		client.RegisterHandler(ChatMsg.Login, OnLogin);
		client.RegisterHandler(ChatMsg.ChannelCreate, OnChannelCreate);
		client.RegisterHandler(ChatMsg.ChannelJoin, OnChannelJoin);
		client.RegisterHandler(ChatMsg.ChannelLeave, OnChannelLeave);
		client.RegisterHandler(ChatMsg.Talk, OnTalk);
		client.RegisterHandler(ChatMsg.ListChannels, OnListChannels);

		client.Connect(chatServerHost, chatServerPort);
	}


	public void Login()
	{
		var msg = new LoginMessage();
		msg.personName = chatName;

		client.Send(ChatMsg.Login, msg);
		Debug.Log("client login");
	}

	void OnConnect(NetworkMessage netMsg)
	{
		Login();
	}

	void OnLogin(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<LoginResponseMessage>();
		myChatPerson = new ChatPerson(msg.personName, msg.personId);
		people[myChatPerson.chatPersonId] = myChatPerson;

		Debug.Log("Client Login myChatPerson " + myChatPerson.chatPersonId);
	}

	void OnChannelCreate(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<ChannelCreateResponseMessage>();

		var channel = new ChatChannel(msg.channelName, msg.channelId);
		channel.ClientAdd(myChatPerson);
		channels[channel.chatChannelId] = channel;

		var person = people[msg.personId];
		channel.ClientAdd(person);

		Debug.Log("Client created channel " + msg.channelId + " " + myChatPerson.chatPersonId);

	}

	void OnChannelJoin(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<ChannelJoinResponseMessage>();

		ChatChannel channel;
		if (channels.ContainsKey(msg.channelId))
		{
			channel = channels[msg.channelId];
		}
		else
		{
			channel = new ChatChannel(msg.channelName, msg.channelId);
			channels[channel.chatChannelId] = channel;
		}

		if (!people.ContainsKey(msg.personId))
		{
			var newPerson = new ChatPerson(msg.personName, msg.personId);
			people[newPerson.chatPersonId] = newPerson;
		}
		var person = people[msg.personId];
		channel.ClientAdd(person);

		Debug.Log("Client joined channel " + msg.channelId + " " + person.chatPersonId);
	}


	void OnChannelLeave(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<ChannelLeaveResponseMessage>();
		//chatChannelId = msg.channelId;

		ChatChannel channel;
		if (channels.ContainsKey(msg.channelId))
		{
			channel = channels[msg.channelId];
		}
		else
		{
			Debug.LogError("Leave channel not found:" + msg.channelId);
			return;
		}

		if (msg.personId == myChatPerson.chatPersonId)
		{
			channels.Remove(msg.channelId);
			Debug.Log("Client left channel " + msg.channelId );

			
			return;
		}

		ChatPerson person;
		if (people.ContainsKey(msg.personId))
		{
			person = people[msg.personId];
		}
		else
		{
			Debug.LogError("Leave person not found:" + msg.personId);
			return;
		}

		channel.ClientRemove(person);

		Debug.Log("Other left channel " + msg.channelId + " " + person.chatPersonId);
	}

	void OnTalk(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<TalkMessage>();

		var channel = channels[msg.channelId];
		var person = channel.people[msg.personId];
		Debug.Log("Client Talk: [" + channel.channelName + "] " + person.personName + ": " + msg.text);

		channel.messages.Add(msg);
		if (channel.messages.Count > kMaxChannelMessages)
		{
			channel.messages.RemoveAt(0);
		}
	}

	void OnListChannels(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<ListChannelsResponseMessage>();
		channelList = msg.channels;
	}

	void Update()
	{
	}


	void OnGUI()
	{
		if (client == null)
		{
			if (GUI.Button(new Rect(xoffset + 10, 50, 200, 20), "Start Chat Client"))
			{
				Setup();
			}
		}
		else
		{
			int ypos = 50;

			if (GUI.Button(new Rect(xoffset + 10, ypos, 200, 20), "Stop Chat Client"))
			{
				client.Disconnect();
				client = null;
			}
			ypos += 25;

			if (GUI.Button(new Rect(xoffset + 10, ypos, 120, 20), "List Channels"))
			{
				client.Send(ChatMsg.ListChannels, new EmptyMessage());
			}
			ypos += 25;

			if (channelList != null)
			{
				foreach (var info in channelList)
				{
					if (GUI.Button(new Rect(xoffset + 30, ypos, 120, 20), "Join " + info.channelName))
					{
						var joinMsg = new ChannelJoinRequestMessage();
						joinMsg.personId = myChatPerson.chatPersonId;
						joinMsg.name = info.channelName;

						client.Send(ChatMsg.ChannelJoin, joinMsg);
					}
					ypos += 25;
				}
			}

			createChannelName = GUI.TextField(new Rect(xoffset + 130, ypos, 95, 20), createChannelName);
			if (GUI.Button(new Rect(xoffset + 10, ypos, 120, 20), "Create Channel"))
			{
				var createMsg = new ChannelCreateMessage();
				createMsg.personId = myChatPerson.chatPersonId;
				createMsg.channelName = createChannelName;

				client.Send(ChatMsg.ChannelCreate, createMsg);

				Debug.Log("Create " + createMsg.personId);
			}
			ypos += 25;

			joinChannelName = GUI.TextField(new Rect(xoffset + 130, ypos, 95, 20), joinChannelName);
			if (GUI.Button(new Rect(xoffset + 10, ypos, 120, 20), "Join Channel"))
			{
				var joinMsg = new ChannelJoinRequestMessage();
				joinMsg.personId = myChatPerson.chatPersonId;
				joinMsg.name = joinChannelName;

				client.Send(ChatMsg.ChannelJoin, joinMsg);

				Debug.Log("Join " + joinMsg.personId);
			}
			ypos += 25;

			talkText = GUI.TextField(new Rect(xoffset + 10, ypos, 220, 20), talkText);
			ypos += 25;

			foreach (var c in channels.Values)
			{
				if (GUI.Button(new Rect(xoffset + 30, ypos, 100, 20), "Talk ch:" + c.channelName))
				{
					var talkMsg = new TalkMessage();
					talkMsg.personId = myChatPerson.chatPersonId;
					talkMsg.channelId = c.chatChannelId;
					talkMsg.text = talkText;

					client.Send(ChatMsg.Talk, talkMsg);
				}
				if (GUI.Button(new Rect(xoffset + 140, ypos, 80, 20), "Leave"))
				{
					var leaveMsg = new ChannelLeaveMessage();
					leaveMsg.channelId = c.chatChannelId;
					leaveMsg.personId = myChatPerson.chatPersonId;

					client.Send(ChatMsg.ChannelLeave, leaveMsg);
				}
				ypos += 25;

				foreach (var t in c.messages)
				{
					if (channels.ContainsKey(t.channelId))
					{
						var channel = channels[t.channelId];
						var person = channel.people[t.personId];

						GUI.TextField(new Rect(xoffset + 10, ypos, 400, 20), channel.channelName + ":" + person.personName + ":" + t.text);
						ypos += 25;
					}
				}
			}

		}
	}
}
