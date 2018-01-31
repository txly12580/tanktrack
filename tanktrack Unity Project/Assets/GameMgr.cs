using UnityEngine;
using System.Collections;

public class GameMgr:MonoBehaviour
{
	public static GameMgr instance;
	public string id = "Tank";

	//use this for initialization
	void Awake()
	{
		instance = this;
	}
}