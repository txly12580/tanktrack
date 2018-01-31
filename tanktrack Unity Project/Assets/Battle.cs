using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour {
	private PanelMgr Panel;
	public static Battle instance;
	public BattleTank[] battleTanks;
	public GameObject[] tankPrefabs;

	void Start()
	{
		
		instance = this;
		/*
		StartTwoCampBattle (3, 3);
		*/
	}

	public int GetCamp(GameObject tankObj)
	{
		for (int i = 0; i < battleTanks.Length; i++) 
		{
			BattleTank battleTank = battleTanks [i];
			if (battleTanks == null)
				return 0;
			if (battleTank.tank.gameObject == tankObj)
				return battleTank.camp;
		}
		return 0;
	}

	public bool IsSameCamp(GameObject tank1,GameObject tank2)
	{
		return GetCamp (tank1) == GetCamp (tank2);
	}

	public bool IsWin(int camp)
	{
		for (int i = 0; i < battleTanks.Length; i++) 
		{
			Tank tank = battleTanks [i].tank;
			if (battleTanks[i].camp != camp)
			if (tank.hp > 0)
				return false;
		}
		Debug.Log ("阵营" + camp + "获胜");
		PanelMgr.instance.OpenPanel<WinPanel> ("", camp);
		return true;
	}

	public bool IsWin(GameObject attTank)
	{
		int camp = GetCamp (attTank);
		return IsWin (camp);
	}

	public void ClearBattle()
	{
		GameObject[] tanks = GameObject.FindGameObjectsWithTag ("Tank");
		for (int i = 0; i < tanks.Length; i++)
			Destroy (tanks [i]);
	}

	public void StartTwoCampBattle(int n1,int n2)
	{
		Transform sp = GameObject.Find ("SwopPoints").transform;
		Transform spCamp1 = sp.GetChild (0);
		Transform spCamp2 = sp.GetChild (1);
		if (spCamp1.childCount < n1 || spCamp2.childCount < n2) 
		{
			Debug.LogError ("出生点数量不够");
			return;
		}
		if (tankPrefabs.Length < 2) 
		{
			Debug.LogError ("坦克预设数量不够");
			return;
		}
		ClearBattle ();
		battleTanks = new BattleTank[n1 + n2];
		for (int i = 0; i < n1; i++) 
		{
			GenerateTank (1, i, spCamp1, i);
		}
		for (int i = 0; i < n2; i++) 
		{
			GenerateTank (2, i, spCamp2, n1 + i);
		}

		Tank tankCmp = battleTanks [0].tank;
		tankCmp.ctrlType = Tank.CtrlType.player;
		CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow> ();
		GameObject target = tankCmp.gameObject;
		cf.SetTarget (target);
	}

	public void GenerateTank(int camp,int num,Transform spCamp,int index)
	{
		Transform trans = spCamp.GetChild (num);
		Vector3 pos = trans.position;
		Quaternion rot = trans.rotation;
		GameObject prefab = tankPrefabs [camp - 1];
		GameObject tankObj = (GameObject)Instantiate (prefab, pos, rot);
		Tank tankCmp = tankObj.GetComponent<Tank> ();
		tankCmp.ctrlType = Tank.CtrlType.computer;
		//battleTanks
		battleTanks[index]=new BattleTank();
		battleTanks [index].tank = tankCmp;
		battleTanks [index].camp = camp;
	}
}
