using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : PanelBase{
	private Button closeBtn;
	private Button tcpBtn;

	#region
	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "InfoPanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		closeBtn = skinTrans.Find ("CloseBtn").GetComponent<Button> ();
		closeBtn.onClick.AddListener (OnCloseClick);
	}

	#endregion
	public void OnCloseClick()
	{
		Close ();
	}

	public void OnClicktcp()
	{
		
	}
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
