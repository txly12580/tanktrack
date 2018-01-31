using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : PanelBase{

	private Image winImage;
	private Image failImage;
	private Text text;
	private Button closeBtn;
	private bool isWin;
	#region
	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "WinPanel";
		layer = PanelLayer.Panel;
		if (args.Length == 1) 
		{
			int camp = (int)args [0];
			isWin = (camp == 1);
		}
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		closeBtn = skinTrans.Find ("CloseBtn").GetComponent<Button> ();
		closeBtn.onClick.AddListener (OnCloseClick);
		winImage = skinTrans.Find ("WinImage").GetComponent<Image> ();
		failImage = skinTrans.Find ("FailImage").GetComponent<Image> ();
		text = skinTrans.Find ("Text").GetComponent<Text> ();
		if (isWin) {
			failImage.enabled = false;
			text.text = "Win!";
		} else 
		{
			winImage.enabled = false;
			text.text = "failed";
		}
	}
	#endregion

	public void OnCloseClick()
	{
		Battle.instance.ClearBattle ();
		PanelMgr.instance.OpenPanel<TitlePanel> ("");
		Close ();
	}


	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
