using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	public float speed = 100f;
	public GameObject explode;
	public float maxLiftTime = 2f;
	public float instantiateTime = 0f;
	public GameObject attackTank;

	void Start () {
		instantiateTime = Time.time;
	}
	void Update () {
		transform.position += transform.up * speed * Time.deltaTime;
		if (Time.time - instantiateTime > maxLiftTime)
			Destroy (gameObject);
	}

	void OnCollisionEnter(Collision collisionInfo)
	{
		if (collisionInfo.gameObject == attackTank)
			return;
		Instantiate (explode, transform.position, transform.rotation);
		Destroy (gameObject);
		Tank tank = collisionInfo.gameObject.GetComponent<Tank> ();
		if (tank != null) {
			float att = GetAtt ();
			tank.BeAttacked (att,attackTank);
		}
	}
		
	private float GetAtt()
	{
		float att = 100 - (Time.time - instantiateTime) * 40;
		if (att < 1)
			att = 1;
		return att;
	}
}
