using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoomListPanel:PanelBase
{
	private Text idText;
	private Text winText;
	private Text lostText;
	private Transform content;
	private GameObject roomPrefab;
	private Button closeBtn;
	private Button newBtn;
	private Button reflashBtn;

	#region
	///<summary>初始化</summary>
	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "RoomListPanel";
		layer = PanelLayer.Panel;
	}
	#endregion

	public override void OnShowing()
	{
		base.OnShowing ();
		//Transform
		Transform skinTrans = skin.transform;
		Transform listTrans = skinTrans.Find ("ListImage");
		Transform winTrans = skinTrans.Find ("WinImage");
		//获取成绩栏部件
		idText = winTrans.Find("IDText").GetComponent<Text>();
		winText = winTrans.Find ("WinText").GetComponent<Text> ();
		lostText = winTrans.Find ("LostText").GetComponent<Text> ();
		//获取列表栏部件
		Transform scroolRect = listTrans.Find("ScrollRect");
		content = scroolRect.Find ("Content");
		roomPrefab = content.Find ("RoomPrefab").gameObject;
		roomPrefab.SetActive (false);

		closeBtn = listTrans.Find ("CloseBtn").GetComponent<Button> ();
		newBtn = listTrans.Find ("NewBtn").GetComponent<Button> ();
		reflashBtn = listTrans.Find ("ReflashBtn").GetComponent<Button> ();
		reflashBtn.onClick.AddListener (OnReflashClick);
		newBtn.onClick.AddListener (OnNewClick);
		closeBtn.onClick.AddListener (OnCloseClick);
	
		NetMgr.srvConn.msgDist.AddListener ("GetAchieve", RecvGetAchieve);
		NetMgr.srvConn.msgDist.AddListener ("GetRoomList", RecvGetRoomList);

		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetRoomList");
		NetMgr.srvConn.Send (protocol);

		protocol = new ProtocolBytes ();
		protocol.AddString ("GetAchieve");
		NetMgr.srvConn.Send (protocol);

		ProtocolBytes a = new ProtocolBytes ();
		a.AddString ("GetRoomList");
		a.AddInt (2);

		a.AddInt (2);
		a.AddInt (1);

		a.AddInt (4);
		a.AddInt (2);
		RecvGetRoomList (a);


	}

	public override void OnClosing()
	{
		NetMgr.srvConn.msgDist.DelListener ("GetAchieve", RecvGetAchieve);
		NetMgr.srvConn.msgDist.DelListener ("GetRoomList", RecvGetRoomList);
	}

	public void RecvGetAchieve(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int win = proto.GetInt (start, ref start);
		int lost = proto.GetInt (start, ref start);

		idText.text = "指挥官:" + GameMgr.instance.id;
		winText.text = win.ToString ();
		lostText.text = lost.ToString ();
	}

	public void RecvGetRoomList(ProtocolBase protocol)
	{
		ClearRoomUnit ();
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int count = proto.GetInt (start, ref start);
		for (int i = 0; i < count; i++) 
		{
			int num = proto.GetInt (start, ref start);
			int status = proto.GetInt (start, ref start);
			GenerateRoomUnit (i, num, status);
		}
	}

	public void ClearRoomUnit()
	{
		for (int i = 0; i < content.childCount; i++)
			if (content.GetChild (i).name.Contains ("Clone"))
				Destroy (content.GetChild (i).gameObject);
	}

	public void GenerateRoomUnit(int i,int num,int status)
	{
		content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, (i + 1) * 110);
		GameObject o = Instantiate (roomPrefab);
		o.transform.SetParent (content);
		o.SetActive (true);
	
		Transform trans = o.transform;
		Text nameText = trans.Find ("nameText").GetComponent<Text> ();
		Text countText = trans.Find ("CountText").GetComponent<Text> ();
		Text statusText = trans.Find ("StatusText").GetComponent<Text> ();
		nameText.text = "序号:" + (i + 1).ToString ();
		countText.text = "人数：" + num.ToString ();
		if (status == 1) {
			statusText.color = Color.black;
			statusText.text = "状态：准备中";
		} else {
			statusText.color = Color.red;
			statusText.text = "状态：开战中";
		}
		Button btn = trans.Find("JoinButton").GetComponent<Button> ();
		btn.name = i.ToString ();
		btn.onClick.AddListener (delegate() {
			OnJoinBtnClick (btn.name);
		});
	}

	public void OnReflashClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetRoomList");
		NetMgr.srvConn.Send (protocol);
	}

	public void OnJoinBtnClick(string name)
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("EnterRoom");

		protocol.AddInt (int.Parse (name));
		NetMgr.srvConn.Send (protocol, OnJoinBtnBack);
		Debug.Log ("请求进入房间" + name);
	}

	public void OnJoinBtnBack(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);
		//处理
		if (ret == 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "成功进入房间！");
			PanelMgr.instance.OpenPanel<RoomPanel> ("");
			Close ();
		} else 
		{
			PanelMgr.instance.OpenPanel<TipPanel> ("", "进入房间失败！");
		}
	}

	public void OnNewClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("CreateRoom");
		NetMgr.srvConn.Send (protocol, OnNewBack);
	}

	public void OnNewBack(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);
		//处理
		if (ret == 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "创建成功！");
			PanelMgr.instance.OpenPanel<RoomPanel> ("");
			Close ();
		} else {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "创建房间失败！");
		}
	}

	public void OnCloseClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Logout");
		NetMgr.srvConn.Send (protocol, OnCloseBack);
	}

	public void OnCloseBack(ProtocolBase protocol)
	{
		PanelMgr.instance.OpenPanel<TipPanel> ("", "登出成功！");
		PanelMgr.instance.OpenPanel<LoginPanel> ("","");
		//NetMgr.srvConn.Close ();
	}
}