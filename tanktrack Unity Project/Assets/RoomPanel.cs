using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RoomPanel : PanelBase
{
	private List<Transform> prefabs = new List<Transform> ();
	private Button closeBtn;
	private Button startBtn;

	#region
	///<summary>初始化</summary>
	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "RoomPanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		//组件
		for (int i = 0; i < 6; i++) 
		{
			string name = "PlayerPrefab" + i.ToString ();
			Transform prefab = skinTrans.Find (name);
			prefabs.Add (prefab);
		}
		closeBtn = skinTrans.Find ("CloseBtn").GetComponent<Button> ();
		startBtn = skinTrans.Find ("StartBtn").GetComponent<Button> ();
		closeBtn.onClick.AddListener (OnCloseClick);
		startBtn.onClick.AddListener (OnStartClick);

		NetMgr.srvConn.msgDist.AddListener ("GetRoomInfo", RecvGetRoomInfo);
		NetMgr.srvConn.msgDist.AddListener ("Fight", RecvFight);

		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetRoomInfo");
		NetMgr.srvConn.Send (protocol);


		ProtocolBytes a = new ProtocolBytes ();
		a.AddString ("GetRoomInfo");
		a.AddInt (2);

		a.AddString ("Killer");
		a.AddInt (1);
		a.AddInt (15);
		a.AddInt (18);
		a.AddInt (0);

		a.AddString ("FireGod");
		a.AddInt (2);
		a.AddInt (3);
		a.AddInt (8);
		a.AddInt (1);
		RecvGetRoomInfo (a);
	}
	#endregion

	public override void OnClosing()
	{
		NetMgr.srvConn.msgDist.DelListener ("GetRoomInfo", RecvGetRoomInfo);
		NetMgr.srvConn.msgDist.DelListener ("Fight", RecvFight);
	}

	public void RecvGetRoomInfo(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int count = proto.GetInt (start, ref start);
		int i = 0;
		for (i = 0; i < count; i++) {
			string id = proto.GetString (start, ref start);
			int team = proto.GetInt (start, ref start);
			int win = proto.GetInt (start, ref start);
			int fail = proto.GetInt (start, ref start);
			int isOwner = proto.GetInt (start, ref start);

			Transform trans = prefabs [i];
			Text text = trans.Find ("Text").GetComponent<Text> ();
			string str = "名字:"+id+"\r\n";
			str+="阵营:"+(team == 1 ? "红":"蓝")+"\r\n";
			str+="胜利:"+win.ToString()+"\r\n";
			str+="失败:"+fail.ToString()+"\r\n";
			if(id == GameMgr.instance.id)
				str+="【我自己】";
			if(isOwner == 1)
				str+="【房主】";
			text.text = str;

			if (team == 1)
				trans.GetComponent<Image> ().color = Color.red;
			else
				trans.GetComponent<Image> ().color = Color.blue;
		}

		for (; i < 6; i++) {
			Transform trans = prefabs [i];
			Text text = trans.Find ("Text").GetComponent<Text> ();
			text.text = "【等待玩家】";
			trans.GetComponent<Image> ().color = Color.gray;
		}
	}

	public void OnCloseClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("LeaveRoom");
		NetMgr.srvConn.Send (protocol, OnCloseBack);
	}

	public void OnCloseBack(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);
		if (ret == 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "退出成功！");
			PanelMgr.instance.OpenPanel<RoomListPanel> ("");
		} else {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "退出失败！");
		}
	}

	public void OnStartClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("StartFight");
		NetMgr.srvConn.Send (protocol, OnStartBack);
	}

	public void OnStartBack(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);
		if (ret != 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "开始游戏失败！两队至少都需要一名玩家，只有队长可以开始战斗！");
		}
	}

	public void RecvFight(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		MultiBattle.instance.StartBattle (proto);
		Close ();
	}
}