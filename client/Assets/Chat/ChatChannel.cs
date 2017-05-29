using System;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine.Networking;

[Serializable]
public struct ChatChannelId
{
	public int value;

	public override string ToString()
	{
		return "ChatChannel:" + value;
	}

	public override int GetHashCode()
	{
		return (int)value;
	}

	public override bool Equals(object obj)
	{
		return obj is ChatChannelId && this == (ChatChannelId)obj;
	}

	public static bool operator ==(ChatChannelId c1, ChatChannelId c2)
	{
		return c1.value == c2.value;
	}

	public static bool operator !=(ChatChannelId c1, ChatChannelId c2)
	{
		return c1.value != c2.value;
	}
}

[Serializable]
public struct ChatPersonId
{
	public int value;

	public override string ToString()
	{
		return "ChatPerson:" + value;
	}

	public override int GetHashCode()
	{
		return (int)value;
	}

	public override bool Equals(object obj)
	{
		return obj is ChatPersonId && this == (ChatPersonId)obj;
	}

	public static bool operator ==(ChatPersonId c1, ChatPersonId c2)
	{
		return c1.value == c2.value;
	}

	public static bool operator !=(ChatPersonId c1, ChatPersonId c2)
	{
		return c1.value != c2.value;
	}
}

[Serializable]
public class ChatPerson
{
	static int nextPersonId = 2000;

	// on client
	public ChatPerson(string name, ChatPersonId id)
	{
		chatPersonId = id;
		personName = name;
	}

	// on server
	public ChatPerson(string name, NetworkConnection conn)
	{
		chatPersonId.value = nextPersonId++;
		personName = name;
		connection = conn;
	}
	public ChatPersonId chatPersonId;
	public string personName;
	public NetworkConnection connection; // only valid on server
}

[Serializable]
public class ChatChannel
{
	static int nextChannelId = 1000;

	public ChatChannelId chatChannelId;
	public string channelName;
	public ChatPerson channelOwner;
	public Dictionary<ChatPersonId, ChatPerson> people = new Dictionary<ChatPersonId, ChatPerson>();
	public List<ChatPerson> peopleList = new List<ChatPerson>();

	public List<TalkMessage> messages = new List<TalkMessage>(); 

	// on client
	public ChatChannel(string name, ChatChannelId id)
	{
		chatChannelId = id;
		channelName = name;
	}

	// on server
	public ChatChannel(string name, ChatPerson owner)
	{
		chatChannelId.value = nextChannelId++;
		channelName = name;
		channelOwner = owner;
	}

	public bool ServerCreate(ChatPerson person)
	{
		people[person.chatPersonId] = person;
		peopleList.Add(person);
		return true;
	}

	public bool ServerJoin(ChatPerson person)
	{
		if (people.ContainsKey(person.chatPersonId))
		{
			Debug.LogError("Join: already in channel " + person.chatPersonId);
			return false;
		}

		// send new person msg to all
		var outMsg = new ChannelJoinResponseMessage();
		outMsg.channelName = channelName;
		outMsg.channelId = chatChannelId;
		outMsg.personId = person.chatPersonId;
		outMsg.personName = person.personName;

		foreach (var other in peopleList)
		{
			other.connection.Send(ChatMsg.ChannelJoin, outMsg);
		}

		people[person.chatPersonId] = person;
		peopleList.Add(person);

		// send existing people to new person
		foreach (var other in peopleList)
		{
			outMsg.personId = other.chatPersonId;
			outMsg.personName = other.personName;
			person.connection.Send(ChatMsg.ChannelJoin, outMsg);
		}
		return true;
	}

	public bool ServerLeave(ChatPerson person)
	{
		if (!people.ContainsKey(person.chatPersonId))
		{
			Debug.LogError("Leave: not in channel " + person.chatPersonId);
			return false;
		}

		// send leave msg to all
		var outMsg = new ChannelLeaveResponseMessage();
		outMsg.channelId = chatChannelId;
		outMsg.personId = person.chatPersonId;

		foreach (var other in peopleList)
		{
			other.connection.Send(ChatMsg.ChannelLeave, outMsg);
		}

		people.Remove(person.chatPersonId);
		peopleList.Remove(person);

		if (peopleList.Count == 0)
		{
			//TODO: remove empty channel?
		}

		return true;
	}

	public bool ServerSay(ChatPerson person, string text)
	{
		if (!people.ContainsKey(person.chatPersonId))
		{
			Debug.LogError("Say: not in channel " + person.chatPersonId);
			return false;
		}

		// send leave msg to others
		var outMsg = new TalkMessage();
		outMsg.channelId = chatChannelId;
		outMsg.personId = person.chatPersonId;
		outMsg.text = text;

		foreach (var other in peopleList)
		{
			other.connection.Send(ChatMsg.Talk, outMsg);
		}
		return true;
	}

	public void ClientAdd(ChatPerson person)
	{
		people[person.chatPersonId] = person;
		peopleList.Add(person);
	}

	public void ClientRemove(ChatPerson person)
	{
		//people.Remove(person.chatPersonId); - keep these so that old messages can refer to them
		peopleList.Remove(person);
	}
}
