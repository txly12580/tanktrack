using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour {

	void OnGUI()
	{
		AudioSource audio = GetComponent<AudioSource> ();
		if (GUI.Button (new Rect (0, 150, 100, 50), "开始"))
			audio.Play ();
		if (GUI.Button (new Rect (100, 150, 100, 50), "停止"))
			audio.Stop ();
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
