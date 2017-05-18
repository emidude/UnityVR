#region License
/*
 * TestSocketIO.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 Fabio Panettieri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System.Collections;
using UnityEngine;
using SocketIO;

public class TestSocketIO : MonoBehaviour
{
	[SerializeField]
	private SocketIOComponent socket;

	private string userId;

	public void Start() 
	{
		userId = "user" + Random.Range (0, 99999999);
		socket.On("open", TestOpen);
		socket.On("playVideo", PlayVideo);
		socket.On("error", TestError);
		socket.On("close", TestClose);
		
	}

	void PlayVideo (SocketIOEvent obj)
	{
		throw new System.NotImplementedException ();
	}

	public void TestOpen(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
		JSONObject json = new JSONObject (JSONObject.Type.OBJECT);
		json.AddField ("userId", userId);
		socket.Emit ("register", json);
	}
		
	public void TestError(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
	}
	
	public void TestClose(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}
}
