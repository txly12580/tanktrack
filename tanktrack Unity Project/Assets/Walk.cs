using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class Walk:MonoBehaviour
{
	public GameObject prefab;
	//players
	Dictionary<string,GameObject>players = new Dictionary<string,GameObject>();
	//self
	string playerID = "";
	public float lastMoveTime;
	public static Walk instance;
	void Start()
	{
		instance = this;
	}

	void AddPlayer(string id,Vector3 pos,int score)
	{
		GameObject player = (GameObject)Instantiate (prefab, pos, Quaternion.identity);
		TextMesh textMesh = player.GetComponentInChildren<TextMesh> ();
		textMesh.text = id + ":" + score;
		players.Add (id, player);
	}

	void DelPlayer(string id)
	{
		if(players.ContainsKey(id))
		{
			Destroy(players[id]);
			players.Remove(id);
		}
	}

	public void UpdateScore(string id,int score)
	{
		GameObject player = players [id];
		if (player == null)
			return;
		TextMesh testMesh = player.GetComponentInChildren<TextMesh> ();
		testMesh.text = id + ":" + score;
	}

	public void UpdateInfo(string id,Vector3 pos,int score)
	{
		if (id == playerID) 
		{
			UpdateScore (id, score);
			return;
		}

		if (players.ContainsKey (id)) {
			players [id].transform.position = pos;
			UpdateScore (id, score);
		} 
		else 
		{
			AddPlayer (id, pos, score);
		}
	}

	public void StartGame(string id)
	{
		playerID = id;
		UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
		float x = 100 + UnityEngine.Random.Range (-30, 30);
		float y = 0;
		float z = 100 + UnityEngine.Random.Range (-30, 30);
		Vector3 pos = new Vector3 (x, y, z);
		AddPlayer (playerID, pos, 0);
		SendPos ();

		ProtocolBytes proto = new ProtocolBytes ();
		proto.AddString ("GetList");
		NetMgr.srvConn.Send (proto, GetList);
		NetMgr.srvConn.msgDist.AddListener ("UpdateInfo", UpdateInfo);
		NetMgr.srvConn.msgDist.AddListener ("PlayerLeave", PlayerLeave);
	}

	void SendPos()
	{
		GameObject player = players [playerID];
		Vector3 pos = player.transform.position;

		ProtocolBytes proto = new ProtocolBytes ();
		proto.AddString ("UpdateInfo");
		proto.AddFloat (pos.x);
		proto.AddFloat (pos.y);
		proto.AddFloat (pos.z);
		NetMgr.srvConn.Send (proto);
	}

	public void GetList(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		int count = proto.GetInt (start, ref start);
		for (int i = 0; i < count; i++) 
		{
			string id = proto.GetString (start, ref start);
			float x = proto.GetFloat (start, ref start);
			float y = proto.GetFloat (start, ref start);
			float z = proto.GetFloat (start, ref start);
			int score = proto.GetInt (start, ref start);
			Vector3 pos = new Vector3 (x, y, z);
			UpdateInfo (id, pos, score);
		}
	}

	public void UpdateInfo(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		string id = proto.GetString (start, ref start);
		float x = proto.GetFloat (start, ref start);
		float y = proto.GetFloat (start, ref start);
		float z = proto.GetFloat (start, ref start);
		int score = proto.GetInt (start, ref start);
		Vector3 pos = new Vector3 (x, y, z);
		UpdateInfo (id, pos, score);
	}

	public void PlayerLeave(ProtocolBase protocol)
	{
		ProtocolBytes proto = (ProtocolBytes)protocol;
		int start = 0;
		string protoName = proto.GetString (start, ref start);
		string id = proto.GetString (start, ref start);
		DelPlayer (id);
	}

	void Move()
	{
		if (playerID == "")
			return;
		if (players [playerID] == null)
			return;
		if (Time.time - lastMoveTime < 0.1)
			return;
		lastMoveTime = Time.time;

		GameObject player = players [playerID];
		if (Input.GetKey (KeyCode.UpArrow)) {
			player.transform.position += new Vector3 (0, 0, 1);
			SendPos ();
		} else if (Input.GetKey (KeyCode.DownArrow)) {
			player.transform.position += new Vector3 (0, 0, -1);
			SendPos ();
		} else if (Input.GetKey (KeyCode.LeftArrow)) {
			player.transform.position += new Vector3 (-1, 0, 0);
			SendPos ();
		} else if (Input.GetKey (KeyCode.RightArrow)) {
			player.transform.position += new Vector3 (1, 0, 0);
			SendPos ();
		}
		else if (Input.GetKey (KeyCode.Space))
		{
			ProtocolBytes proto = new ProtocolBytes ();
			proto.AddString ("AddScore");
			NetMgr.srvConn.Send (proto);
		}
	}

	void update()
	{
		Move ();
	}
}









/*using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class Walk : MonoBehaviour {
	Socket socket;
	const int BUFFER_SIZE = 1024;
	public byte[] readBuff = new byte[BUFFER_SIZE];

	Dictionary<string,GameObject>
	players = new Dictionary<string,GameObject>();

	List<string> msgList = new List<string>();
	public GameObject prefab;
	string id;

	void AddPlayer(string id,Vector3 pos)
	{
		GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
		TextMesh textMesh = player.GetComponentInChildren<TextMesh> ();
		textMesh.text = id;
		players.Add (id, player);
	}

	void SendPos()
	{
		GameObject player = players [id];
		Vector3 pos = player.transform.position;
		string str = "POS";
		str += id + " ";
		str += pos.x.ToString () + " ";
		str += pos.y.ToString () + " ";
		str += pos.z.ToString () + " ";

		byte[] bytes = System.Text.Encoding.Default.GetBytes (str);
		socket.Send (bytes);
		Debug.Log ("发送" + str);
	}

	void SendLeave()
	{
		string str = "LEAVE";
		str += id + " ";
		byte[] bytes = System.Text.Encoding.Default.GetBytes (str);
		socket.Send (bytes);
		Debug.Log ("发送" + str);
	}

	void Move()
	{
		if (id == "")
			return;
		GameObject player = players[id];

		if (Input.GetKey (KeyCode.UpArrow)) 
		{
			player.transform.position += new Vector3 (0, 0, 1);
			SendPos ();
		} 
		else if (Input.GetKey (KeyCode.DownArrow)) 
		{
			player.transform.position += new Vector3 (0, 0, -1);
			SendPos ();
		}
		else if (Input.GetKey (KeyCode.LeftArrow)) 
		{
			player.transform.position += new Vector3 (-1, 0, 0);
			SendPos ();
		}
		else if (Input.GetKey (KeyCode.RightArrow)) 
		{
			player.transform.position += new Vector3 (1, 0, 0);
			SendPos ();
		}
	}

	void OnDestroy()
	{
		SendLeave ();
	}

	void Start()
	{
		Connect ();
		UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
		float x = 100 + UnityEngine.Random.Range (-30, 30);
		float y = 0;
		float z = 100 + UnityEngine.Random.Range (-30, 30);
		Vector3 pos = new Vector3 (x, y, z);
		AddPlayer (id, pos);
		SendPos ();
	}

	void Connect()
	{
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Connect ("127.0.0.1", 1234);
		id = socket.LocalEndPoint.ToString ();
		socket.BeginReceive (readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
	}

	private void ReceiveCb(IAsyncResult ar)
	{
		try
		{
			int count = socket.EndReceive(ar);
			string str = System.Text.Encoding.UTF8.GetString(readBuff,0,count);
			msgList.Add(str);
			socket.BeginReceive(readBuff,0,BUFFER_SIZE,SocketFlags.None,ReceiveCb,null);
		}
		catch(Exception e) 
		{
			socket.Close ();
		}

	}

	void Update()
	{
		for(int i = 0; i<msgList.Count;i++)
			HandleMsg();
		Move();
	}

	void HandleMsg()
	{
		if (msgList.Count <= 0)
			return;
		string str = msgList [0];
		msgList.RemoveAt (0);
		string[] args = str.Split(' ');
		if(args[0]=="POS")
		{
			OnRecvPos(args[1],args[2],args[3],args[4]);
		}
		else if(args[0]=="LEAVE")
		{
			OnRecvLeave(args[1]);
		}
	}

	public void OnRecvPos(string id,string xStr,string yStr,string zStr)
	{
		if (id == this.id)
			return;
		float x = float.Parse (xStr);
		float y = float.Parse (yStr);
		float z = float.Parse (zStr);
		Vector3 pos = new Vector3 (x, y, z);
		if (players.ContainsKey (id)) 
		{
			players [id].transform.position = pos;
		}
		else 
		{
			AddPlayer (id, pos);
		}
	}

	public void OnRecvLeave(string id)
	{
		if (players.ContainsKey (id)) 
		{
			Destroy (players [id]);
			players [id] = null;
		}
	}
}*/
