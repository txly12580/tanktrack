using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScene : MonoBehaviour {
	void OnGUI()
	{
		if (GUI.Button (new Rect (0, 0, 100, 100), "切换")) {
			Application.LoadLevel ("stronghold");
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
