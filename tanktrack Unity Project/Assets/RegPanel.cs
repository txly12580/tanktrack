using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RegPanel : PanelBase
{
	private InputField idInput;
	private InputField pwInput;
	private InputField repInput;
	private Button regBtn;
	private Button closeBtn;

	#region
	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "RegPanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		idInput = skinTrans.Find ("IDInput").GetComponent<InputField> ();
		pwInput = skinTrans.Find ("PWInput").GetComponent<InputField> ();
		repInput = skinTrans.Find ("ReInput").GetComponent<InputField> ();
		regBtn = skinTrans.Find ("RegBtn").GetComponent<Button> ();
		closeBtn = skinTrans.Find ("CloseBtn").GetComponent<Button> ();

		regBtn.onClick.AddListener (OnRegClick);
		closeBtn.onClick.AddListener (OnCloseClick);
	}
	#endregion

	public void OnCloseClick()
	{
		PanelMgr.instance.OpenPanel<LoginPanel> ("");
		Close ();
	}

	public void OnRegClick()
	{
		if (idInput.text == "" || pwInput.text == "") 
		{
			PanelMgr.instance.OpenPanel<TipPanel> ("", "用户名密码不能为空！");
			//Debug.Log ("用户名密码不能为空！");
			return;
		}

		if (pwInput.text != repInput.text) 
		{
			PanelMgr.instance.OpenPanel<TipPanel> ("", "两次输入的密码不同！");
			return;
		}

		if (NetMgr.srvConn.status != Connection.Status.Connected) 
		{
			string host = "127.0.0.1";
			int port = 1234;
			NetMgr.srvConn.proto = new ProtocolBytes ();
			if (!NetMgr.srvConn.Connect (host, port))
				PanelMgr.instance.OpenPanel <TipPanel> ("", "连接服务器失败！");
			//NetMgr.srvConn.Connect (host, port);
		}
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Register");
		protocol.AddString (idInput.text);
		protocol.AddString (pwInput.text);
		Debug.Log ("发送" + protocol.GetDesc ());
		NetMgr.srvConn.Send (protocol, OnRegBack);
	}

	public void OnRegBack(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int ret = proto.GetInt (start, ref start);
		if (ret == 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "注册成功！");
			//Debug.Log ("注册成功！");
			PanelMgr.instance.OpenPanel<LoginPanel> ("");
			Close ();
		}
		else 
		{
			PanelMgr.instance.OpenPanel<TipPanel> ("", "注册失败，请更换用户名！");
			//Debug.Log ("注册失败！");
		}
	}
}