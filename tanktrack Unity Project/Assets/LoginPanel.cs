using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginPanel:PanelBase
{
	private InputField idInput;
	private InputField pwInput;
	private Button loginBtn;
	private Button regBtn;
	#region
	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "LoginPanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		idInput = skinTrans.Find ("IDInput").GetComponent<InputField> ();
		pwInput = skinTrans.Find ("PWInput").GetComponent<InputField> ();
		loginBtn = skinTrans.Find ("LoginBtn").GetComponent<Button> ();
		regBtn = skinTrans.Find ("RegBtn").GetComponent<Button> ();

		loginBtn.onClick.AddListener (OnLoginClick);
		regBtn.onClick.AddListener (OnRegClick);
	}
	#endregion

	public void OnRegClick()
	{
		PanelMgr.instance.OpenPanel<RegPanel> ("");
		Close ();
	}

	public void OnLoginClick()
	{
		if (idInput.text == "" || pwInput.text == "") 
		{
			PanelMgr.instance.OpenPanel<TipPanel> ("", "用户名密码不能为空！");
			return;
		}
		if (NetMgr.srvConn.status != Connection.Status.Connected) 
		{
			string host = "127.0.0.1";
			int port = 1234;
			NetMgr.srvConn.proto = new ProtocolBytes ();
			if (!NetMgr.srvConn.Connect (host, port))
				PanelMgr.instance.OpenPanel<TipPanel> ("", "连接服务器失败！");
			//NetMgr.srvConn.Connect (host, port);
		}
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Login");
		protocol.AddString (idInput.text);
		protocol.AddString (pwInput.text);
		Debug.Log ("发送" + protocol.GetDesc ());
		NetMgr.srvConn.Send (protocol, OnLoginBack);
	}

	public void OnLoginBack(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);
		if (ret == 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "登录成功！");

			PanelMgr.instance.OpenPanel<RoomListPanel> ("");
			GameMgr.instance.id = idInput.text;
			//Debug.Log ("登录成功！");
			//Walk.instance.StartGame (idInput.text);
			Close ();
		}
		else 
		{
			//Debug.Log ("登录失败！");
			PanelMgr.instance.OpenPanel<TipPanel>("","登录失败，请检查用户名密码！");
		}
	}
}


/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginPanel : MonoBehaviour {

	public string userName = " ";
	public string password = " ";

	void OnGUI()
	{
		//登录框
		GUI.Box(new Rect(10,10,200,120),"登录框");
		GUI.Label (new Rect (20, 40, 50, 30), "用户名");
		userName = GUI.TextField(new Rect(70,40,120,20),userName);
		GUI.Label (new Rect (20, 70, 50, 30), "密码");
		password = GUI.PasswordField(new Rect(70,70,120,20),password,'*');
		if(GUI.Button(new Rect(70,100,50,25),"登录"))
		{
			if(userName == "hellolpy" && password =="123")
				Debug.Log("登录成功");
			else
				Debug.Log("登录失败");
		}
	}
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}*/
