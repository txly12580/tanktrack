using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.Linq;

public class net : MonoBehaviour {
	ProtocolBase proto = new ProtocolBytes();
	public InputField idInput;
	public InputField pwInput;
	public InputField hostInput;
	public InputField portInput;
	public Text recvText;
	public string recvStr;
	public Text clientText;
	public InputField textInput;
	Socket socket;
	const int BUFFER_SIZE=1024;
	int buffCount =0;
	byte[] lenBytes = new byte[sizeof(UInt32)];
	Int32 msgLength =0;
	public byte[] readBuff = new byte[BUFFER_SIZE];

	void Update()
	{
		recvText.text = recvStr;
	}
	public void Connection()
	{
		recvText.text = "";
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		//Connect
		string host = hostInput.text;
		int port = int.Parse (portInput.text);
		socket.Connect (host, port);
		clientText.text = "客户端地址" + socket.LocalEndPoint.ToString ();
		//Recv
		socket.BeginReceive(readBuff,0,BUFFER_SIZE,SocketFlags.None,ReceiveCb,null);
		/*
		//Socket
		socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
		string host = hostInput.text;
		int port = int.Parse (portInput.text);
		socket.Connect (host, port);
		clientText.text = "客户端地址" + socket.LocalEndPoint.ToString ();
		//send
		string str = "Hello Unity";
		byte[] bytes = System.Text.Encoding.Default.GetBytes (str);
		socket.Send (bytes);
		//Recv
		int count = socket.Receive(readBuff);
		str = System.Text.Encoding.UTF8.GetString (readBuff, 0, count);
		recvText.text = str;
		//close
		socket.Close();
		*/
	}

	private void ReceiveCb(IAsyncResult ar)
	{
		try
		{
			int count = socket.EndReceive(ar);
			//string str = System.Text.Encoding.UTF8.GetString(readBuff,0,count);
			//if(recvStr.Length>300)
				//recvStr = "";
			//recvStr += str + "\n";
			//Debug.Log("recvStr"+recvStr);
			buffCount += count;
			ProcessData();
			socket.BeginReceive(readBuff,0,BUFFER_SIZE,SocketFlags.None,ReceiveCb,null);
		}
		catch(Exception e) 
		{
			recvStr +="连接已断开";
			socket.Close ();
		}
	}

	private void ProcessData()
	{
		if (buffCount < sizeof(Int32))
			return;
		Array.Copy (readBuff, lenBytes, sizeof(Int32));
		msgLength = BitConverter.ToInt32 (lenBytes, 0);
		if (buffCount < msgLength + sizeof(Int32))
			return;
		Console.WriteLine ("处理消息！！");
		ProtocolBase protocol = proto.Decode (readBuff, sizeof(Int32), msgLength);
		HandleMsg (protocol);
		//string str = System.Text.Encoding.UTF8.GetString (readBuff, sizeof(Int32), (int)msgLength);
		//recvStr = str;

		int count = buffCount - msgLength - sizeof(Int32);
		Array.Copy (readBuff, msgLength, readBuff, 0, count);
		buffCount = count;
		if (buffCount > 0) 
		{
			ProcessData ();
		}
	}

	private void HandleMsg(ProtocolBase protoBase)
	{
		//ProtocolBytes proto = (ProtocolBytes)protoBase;
		//Debug.Log ("接收" + proto.GetDesc ());
		ProtocolBytes proto = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);

		Debug.Log ("接收" + proto.GetDesc ());
		recvStr = "接收" + proto.GetName () + " " + ret.ToString ();

	}

	public void OnSendClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("HeartBeat");
		Debug.Log ("发送" + protocol.GetDesc ());
		Send (protocol);
	}

	public void Send(ProtocolBase protocol)
	{
		//string str = textInput.text;
		byte[] bytes = protocol.Encode();  //System.Text.Encoding.UTF8.GetBytes (str);
		byte[] length = BitConverter.GetBytes (bytes.Length);
		byte[] sendbuff = length.Concat (bytes).ToArray ();
		socket.Send (sendbuff);
		//try
		//{
			//socket.Send(bytes);
		//}
		//catch
		//{
		//}
	}

	public void OnLoginClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Login");
		protocol.AddString (idInput.text);
		protocol.AddString (pwInput.text);
		Debug.Log ("发送" + protocol.GetDesc ());
		Send (protocol);
	}

	public void OnAddClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("AddScore");
		Debug.Log ("发送" + protocol.GetDesc ());
		Send (protocol);
	}

	public void OnGetClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetScore");
		Debug.Log ("发送" + protocol.GetDesc ());
		Send (protocol);
	}
}
