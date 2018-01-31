﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetMgr
{
	public static Connection srvConn = new Connection ();
	//public static Connection platformConn = new Connection();
	public static void Update()
	{
		srvConn.Update ();
		//platformConn.Update();
	}

	//心跳
	public static ProtocolBase GetHeatBeatProtocol()
	{
		//具体的发送内容根据服务端设定进行改动
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("HeartBeat");
		return protocol;
	}
}