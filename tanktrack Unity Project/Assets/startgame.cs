using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startgame : MonoBehaviour {
	private AI aistart;
	public void ButtonClick() {
		aistart.InitWaypoint ();
	}
}
