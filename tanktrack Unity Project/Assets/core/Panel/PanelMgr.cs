using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PanelLayer
{
	Panel,
	Tips,
}

public class PanelMgr : MonoBehaviour {
	public static PanelMgr instance;
	private GameObject canvas;
	public Dictionary<string,PanelBase> dict;
	private Dictionary<PanelLayer,Transform> layerDict;

	public void Awake()
	{
		instance = this;
		InitLayer ();
		dict = new Dictionary<string,PanelBase> ();
	}

	private void InitLayer()
	{
		canvas = GameObject.Find ("Canvas");
		if (canvas == null)
			Debug.LogError ("panelMgr.InitLayer fail,canvas is null");
		layerDict = new Dictionary<PanelLayer,Transform> ();
		foreach(PanelLayer pl in Enum.GetValues(typeof(PanelLayer)))
		{
			string name = pl.ToString ();
			Transform transform = canvas.transform.Find(name);
			layerDict.Add(pl,transform);
		}
	}

	public void OpenPanel<T>(string skinPath,params object[] args) where T:PanelBase
		{
			string name = typeof(T).ToString ();
			if (dict.ContainsKey (name))
				return;
			PanelBase panel = canvas.AddComponent<T> ();
			panel.Init (args);
			dict.Add (name, panel);
			skinPath = (skinPath != "" ? skinPath : panel.skinPath);
			GameObject skin = Resources.Load<GameObject> (skinPath);
			if (skin == null)
				Debug.LogError ("panelMgr.OpenPanel fail,skin is null,skinPath = " + skinPath);
			panel.skin = (GameObject)Instantiate (skin);
			Transform skinTrans = panel.skin.transform;
			PanelLayer layer = panel.layer;
			Transform parent = layerDict [layer];
			skinTrans.SetParent (parent, false);
			panel.OnShowing ();
			panel.OnShowed ();
		}

public void ClosePanel(string name)
		{
			PanelBase panel = (PanelBase)dict [name];
			if (panel == null)
				return;
			panel.OnClosing ();
			dict.Remove (name);
			panel.OnClosed ();
			GameObject.Destroy (panel.skin);
			Component.Destroy (panel);
		}




	/*
	public GameObject panel1;
	public GameObject panel2;
	public bool startgame = false;

	public void OnPanel1BtnClick()
	{
		panel1.gameObject.SetActive (false);
		panel2.gameObject.SetActive (false);
		startgame = true;
	}

	public void OnPanel2BtnClick()
	{
		panel1.gameObject.SetActive (false);
		panel2.gameObject.SetActive (true);
	}

	public void OnPanel2BtnClick3()
	{
		panel2.gameObject.SetActive (false);
	}
	*/
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
	