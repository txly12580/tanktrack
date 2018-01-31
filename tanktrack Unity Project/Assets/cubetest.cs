using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubetest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Space)) {
			Rigidbody rigi = gameObject.GetComponent<Rigidbody> ();
			Vector3 force = Vector3.up * 50;
			rigi.AddForce (force);
		}
	}

	void OnCollisionEnter(Collision collisionInfo)
	{
		Debug.Log ("碰撞到" + collisionInfo.gameObject.name);
	}
}
