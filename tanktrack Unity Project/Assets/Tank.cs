using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {
	public Transform turret;
	public Transform gun;
	public GameObject bullet;
	public GameObject destoryEffect;
	public Texture2D centerSight;
	public Texture2D tankSight;
	public Texture2D hpBarBg;
	public Texture2D hpBar;
	public Texture2D killUI;
	private float killUIStartTime = float.MinValue;
	private float turretRotSpeed = 0.6f;
	private float turretRotTarget = 0;
	public List<AxleInfo> axleInfos;
	private float motor = 0;
	public float maxMotorTorque;
	private float brakeTorque =0;
	public float maxBrakeTorque = 100;
	private float steering = 0;
	public float maxSteeringAngle;
	public float lastShootTime = 0;
	private float shootInterval = 0.5f;
	private float maxHp = 100;
	public float hp = 100;
	public AudioSource motorAudioSource;
	public AudioClip motorClip;
	private AI ai;
	Vector3 lPos;
	Vector3 lRot;
	//forecast
	Vector3 fPos;
	Vector3 fRot;
	//时间间隔
	float delta =1;
	float lastRecvInfoTime = float.MinValue;
	public enum CtrlType
	{
		none,
		player,
		computer,
		net,
	}
	public CtrlType ctrlType = CtrlType.player;

	void Start () {
		turret = transform.Find ("turret");
		gun = turret.Find ("gun");
		motorAudioSource = gameObject.AddComponent<AudioSource> ();
		motorAudioSource.spatialBlend = 1;

		if (ctrlType == CtrlType.computer) 
		{
			ai = gameObject.AddComponent<AI> ();
			ai.tank = this;
		}
	}
	
	// Update is called once per frame
	//每帧执行一次
	void Update () {
		if (ctrlType == CtrlType.net) {
			NetUpdate ();
			return;
		}
		playerCtrl ();
		ComputerCtrl ();
		NoneCtrl ();
		foreach (AxleInfo axleInfo in axleInfos) {
			if (axleInfo.steering) 
			{
				axleInfo.leftWheel.steerAngle = steering;
				axleInfo.rightWheel.steerAngle = steering;
			}
			if (axleInfo.motor) {
				axleInfo.leftWheel.motorTorque = motor;
				axleInfo.rightWheel.motorTorque = motor;
			}
			if (true) {
				axleInfo.leftWheel.brakeTorque = brakeTorque;
				axleInfo.rightWheel.brakeTorque = brakeTorque;
			}
		}
		TurretRotation ();
		MotorSound ();
	}

	public void TurretRotation()
	{
		if (Camera.main == null)
			return;
		if (turret == null)
			return;
		float angle = turret.eulerAngles.y - turretRotTarget+90;
		if (angle < 0)
			angle += 360;
		if (angle > turretRotSpeed && angle < 180)
			turret.Rotate (0f, -turretRotSpeed, 0f);
		else if (angle > 180 && angle < 360 - turretRotSpeed)
			turret.Rotate (0f, turretRotSpeed, 0f);
	}

	private float lastSendInfoTime = float.MinValue;
	public void playerCtrl()
	{
		if (ctrlType != CtrlType.player)
			return;
		
		motor = maxMotorTorque * Input.GetAxis ("Vertical");
		steering = maxSteeringAngle * Input.GetAxis ("Horizontal");
		//制动
		brakeTorque =0;
		foreach (AxleInfo axleInfo in axleInfos) {
			if (axleInfo.leftWheel.rpm > 5 && motor < 0)
				brakeTorque = maxBrakeTorque;
			else if (axleInfo.leftWheel.rpm < -5 && motor > 0)
				brakeTorque = maxBrakeTorque;
			continue;
		}
		//turretRotTarget = Camera.main.transform.eulerAngles.y;
		TargetSignPos();

		if (Input.GetMouseButton (0)) 
		  shoot ();

		if (Time.time - lastSendInfoTime > 0.2) {
			lastSendInfoTime = Time.time;
		}
		
	}

	public void ComputerCtrl()
	{
		if (ctrlType != CtrlType.computer)
			return;

		Vector3 rot = ai.GetTurretTarget ();
		turretRotTarget = rot.y;
		if (ai.IsShoot ())
			shoot ();
		//移动
		steering = ai.GetSteering();
		Debug.Log ("steering"+steering);
		motor = ai.GetMotor ();
		brakeTorque = ai.GetBrakeTorque ();
	}

	public void NoneCtrl()
	{
		if (ctrlType != CtrlType.none)
			return;
		motor = 0;
		steering = 0;
		brakeTorque = maxBrakeTorque / 2;
	}

	void MotorSound()
	{
		if (motor != 0 && !motorAudioSource.isPlaying) {
			motorAudioSource.loop = true;
			motorAudioSource.clip = motorClip;
			motorAudioSource.Play ();
		} else if (motor == 0) 
		{
			motorAudioSource.Pause ();
		}
	}

	public void TargetSignPos()
	{
		Vector3 hitPoint = Vector3.zero;
		RaycastHit raycastHit;
		Vector3 centerVec = new Vector3 (Screen.width / 2, Screen.height / 2, 0);
		Ray ray = Camera.main.ScreenPointToRay (centerVec);
		if (Physics.Raycast (ray, out raycastHit, 300.0f)) 
		{
			hitPoint = raycastHit.point;
		}
		else 
		{
			hitPoint = ray.GetPoint (300);
		}
		Vector3 dir = hitPoint - gun.position;
		Quaternion angle = Quaternion.LookRotation (dir);
		turretRotTarget = angle.eulerAngles.y;

		//Transform targetCube = GameObject.Find ("TargetCube").transform;
		//targetCube.position = hitPoint;
	}

	public Vector3 CalExplodePoint()
	{
		Vector3 hitPoint = Vector3.zero;
		RaycastHit hit;
		Vector3 pos = gun.position + gun.forward * 5;
		Ray ray = new Ray (pos, gun.forward);
		if (Physics.Raycast (ray, out hit, 300.0f)) 
		{
			hitPoint = hit.point;
		}
		else 
		{
			hitPoint = ray.GetPoint (300);
		}
		//Transform explodeCube = GameObject.Find ("ExplodeCube").transform;
		//explodeCube.position = hitPoint;
		return hitPoint;
	}

	public void shoot()
	{
		if (Time.time - lastShootTime < shootInterval)
			return;
		if (bullet == null)
			return;
		Vector3 pos = gun.position + gun.forward * 5;
		GameObject bulletObj=(GameObject)Instantiate (bullet, pos, gun.rotation);
		Bullet bulletCmp = bulletObj.GetComponent<Bullet> ();
		if (bulletCmp != null)
			bulletCmp.attackTank = this.gameObject;
		lastShootTime = Time.time;
	}

	public void BeAttacked(float att,GameObject attackTank)
	{
		if (hp <= 0)
			return;
		if (hp > 0) {
			hp -= att;
			if (ai != null) 
			{
				ai.OnAttacked (attackTank);
			}
		}
		if (hp <= 0) {
			GameObject destoryObj = (GameObject)Instantiate (destoryEffect);
			destoryObj.transform.SetParent (transform, false);
			destoryObj.transform.localPosition = Vector3.zero;
			ctrlType = CtrlType.none;
			Debug.Log ("attackTank"+attackTank);
			if (attackTank != null) 
			{
				Tank tankCmp = attackTank.GetComponent<Tank> ();
				Debug.Log ("tankCmp"+tankCmp);
				if (tankCmp != null && tankCmp.ctrlType == CtrlType.player)
					tankCmp.StartDrawKill ();
				//战场结算
				Battle.instance.IsWin(attackTank);
			}
		}
	}

	public void StartDrawKill()
	{
		killUIStartTime = Time.time;
		Debug.Log ("killUIStartTime"+killUIStartTime);
	}

	public void DrawSight()
	{
		Vector3 explodePoint = CalExplodePoint ();
		Vector3 screenPoint = Camera.main.WorldToScreenPoint (explodePoint);
		Rect tankRect = new Rect (screenPoint.x - tankSight.width / 2, Screen.height - screenPoint.y - tankSight.height / 2, tankSight.width, tankSight.height);
		Rect centerRect = new Rect (Screen.width / 2 - centerSight.width / 2, Screen.height / 2 - centerSight.height / 2, centerSight.width, centerSight.height);
		GUI.DrawTexture (centerRect, centerSight);
	}

	public void DrawHp()
	{
		Rect bgRect = new Rect (30, Screen.height - hpBarBg.height - 15, hpBarBg.width, hpBarBg.height);
		GUI.DrawTexture (bgRect, hpBarBg);
		float width = hp * 102 / maxHp;
		Rect hpRect = new Rect (bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
		GUI.DrawTexture (hpRect, hpBar);
		string text = Mathf.Ceil (hp).ToString () + "/" + Mathf.Ceil (maxHp).ToString ();
		Rect textRect = new Rect (bgRect.x + 80, bgRect.y - 10, 50, 50);
		GUI.Label (textRect, text);
	}


	private void DrawKillUI()
	{
		if (Time.time - killUIStartTime < 5f) 
		{
			Rect rect = new Rect (Screen.width / 2 - killUI.width / 2, 30, killUI.width, killUI.height);
			GUI.DrawTexture (rect, killUI);
		}
	}


	void OnGUI()
	{
		if (ctrlType != CtrlType.player)
			return;
		DrawSight ();
		DrawHp ();
		DrawKillUI ();
	}

	public void SendUnitInfo()
	{
		ProtocolBytes proto = new ProtocolBytes ();
		proto.AddString ("UpdateUnitInfo");
		Vector3 pos = transform.position;
		Vector3 rot = transform.eulerAngles;
		proto.AddFloat (pos.x);
		proto.AddFloat (pos.y);
		proto.AddFloat (pos.z);
		proto.AddFloat (rot.x);
		proto.AddFloat (rot.y);
		proto.AddFloat (rot.z);

		float angleY = turretRotTarget;
		proto.AddFloat (angleY);

		//float angleX = turretRollTarget;
		//proto.AddFloat(angleX);

		NetMgr.srvConn.Send (proto);
	}

	public void NetForecastInfo(Vector3 nPos,Vector3 nRot)
	{
		fPos = lPos + (nPos - lPos) * 2;
		fRot = lRot + (nRot - lRot) * 2;
		if (Time.time - lastRecvInfoTime > 0.3f) {
			fPos = nPos;
			fRot = nRot;
		}

		delta = Time.time - lastRecvInfoTime;

		lPos = nPos;
		lRot = nRot;
		lastRecvInfoTime = Time.time;
	}

	public void InitNetCtrl()
	{
		lPos = transform.position;
		lRot = transform.eulerAngles;
		fPos = transform.position;
		fRot = transform.eulerAngles;
		Rigidbody r = GetComponent<Rigidbody> ();
		r.constraints = RigidbodyConstraints.FreezeAll;		
	}

	public void NetUpdate()
	{
		Vector3 pos = transform.position;
		Vector3 rot = transform.eulerAngles;

		if (delta > 0) {
			transform.position = Vector3.Lerp (pos, fPos, delta);
			transform.rotation = Quaternion.Lerp (Quaternion.Euler (rot), Quaternion.Euler (fRot), delta);
		}
	}
}
