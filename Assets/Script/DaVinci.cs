using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

public class DaVinci : MonoBehaviour {

	public Sprite[] ht;
	public CardPool[] players;
	public CardEnv[] pool;
	public static Card keyCard;
	public GUIText tip, tName;
	public static GUIText info; 
	public static Sprite WH, WT, BH, BT;
	public static bool tempDrawed;
	public AudioClip win, lose, sensor, pivot, draw;
	private AudioSource sc = null;
	string ip;
	int port = 25002;
	static bool playing, gameStarted;
	List<int> op;
	Card cTemp;
	static int num, cnt, current;

	// Use this for initialization
	void Start () {
		info = tip;
		WH = ht[0];
		WT = ht[1];
		BH = ht[2];
		BT = ht[3];
		sc = gameObject.AddComponent<AudioSource>();
		sc.clip = sensor;
		StartCoroutine(DVC());
	}

	IEnumerator DVC() {
		yield return StartCoroutine(connect());
		StartCoroutine(deal());
	}

	IEnumerator connect() {
		op = new List<int>();
		ip = getIP();
		string tip = ip.Remove(ip.LastIndexOf('.')+1);
		int local = int.Parse(ip.Substring(ip.LastIndexOf('.')+1, ip.Length-ip.LastIndexOf('.')-1));
		if ((Application.platform == RuntimePlatform.Android && local == 1) || 
		    Application.platform == RuntimePlatform.WindowsEditor) {
			serverInit();
		} else {
			DaVinci.info.text = ip + ": 连接中";
			if (Application.platform == RuntimePlatform.Android) connect(tip+1);
			else { connect(ip); }
			/*while(true) {
				if (Network.peerType == NetworkPeerType.Client) {
					break;
				}
				yield return null;
			}/**/
		}/**
		bool connected = false;
		for (int i = 1; i < 256; i++) {
			if (i == local) continue;
			connect("127.0.0.1");
			if (Network.peerType == NetworkPeerType.Client) {
				DaVinci.info.text = ip + ": Connected to " + tip+i + " as client";
				connected = true;
				break;
			}
			Network.Disconnect();
			DaVinci.info.text = ip + ": Connection to " + tip+i + " aborted";
		}
		if (!connected) {
			serverInit();
			DaVinci.info.text = ip + ": Local server initialized";
		}/**
		HostData[] data = MasterServer.PollHostList();
		if (data.Length > 0) {
			Network.Connect(data[0]);
		} else {
			serverInit();
			DaVinci.info.text = ip + ": Local server initialized";
		}/**/
		while(true) {
			if (gameStarted) yield break;
			if (Network.peerType == NetworkPeerType.Client) {
				DaVinci.info.text = "等待服主开始游戏(目前" + cnt + "位玩家已连接)";
			} else if (Network.peerType == NetworkPeerType.Server) {
				if (Network.connections.Length > 0) {
					sendLength();
					DaVinci.info.text = ip + ": 等待玩家(目前" + (Network.connections.Length+1) + "位玩家已连接) 点击上方按钮开始";
				} else DaVinci.info.text = ip + ": 等待玩家(目前" + (Network.connections.Length+1) + "位玩家已连接)";
			}
			yield return null;
		}
	}

	IEnumerator deal() {
		yield return new WaitForSeconds(0.5f);
		DaVinci.info.text = "发牌...";
		if (Network.peerType == NetworkPeerType.Server) {
			for (int j = 0; j <= Network.connections.Length; j++) {
				int t = cnt<4 ? 4:3;
				for (int i = 0; i < t; i++) {
					yield return new WaitForSeconds(0.03f);				
					int c = Random.Range(0,2)%2, v = pool[c].getRandomNum();
					GetComponent<NetworkView>().RPC("RDraw", RPCMode.All, j, c, v);
				}
			}
		} else {
			yield return StartCoroutine(waitForDealing());
		}
		yield return new WaitForSeconds(0.5f);
		DaVinci.info.text = "整牌...";
		players[0].sort();
		yield return new WaitForSeconds(1f);
		if (Network.peerType == NetworkPeerType.Server) 
			GetComponent<NetworkView>().RPC("CurrentTurn", RPCMode.All, 0);
		while (isPlaying()) {
			if (current == num) {
				if (pool[0].Count()==0 && pool[1].Count()==0) 
					DaVinci.info.text = "轮到玩家"+current+"猜牌（已无新牌可取）";
				else DaVinci.info.text = "轮到玩家"+current+"猜牌";
				playing = true;
				while (playing) {
					if (!isPlaying()) break;
					yield return null;
				}
			} else {
				DaVinci.info.text = "等待玩家"+current+"猜牌";
				playing = false;
				while (current != num) {
					if (!isPlaying()) break;
					yield return null;
				}
			}
		}
		StartCoroutine(gameOver());
	}
	
	IEnumerator Sensor() {
		if (Application.platform == RuntimePlatform.Android) {
			bool isUpright;
			/**/
			while (true) {
				isUpright = Mathf.Abs(Input.acceleration.z) < 0.5;
				if (isUpright ^ players[0].visible) {
					if (isUpright) sc.PlayOneShot(sensor);
					flush(isUpright);
				}
				yield return null;
			}
			/**
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			while (true) {
				isUpright = jo.Call<bool>("isUpright");
				if (isUpright ^ players[0].visible) flush(isUpright);
				yield return null;
			}/**/
		} else {
			bool isUpright = false;
			while (true) {
				isUpright = Input.GetKey(KeyCode.F1);
				if (isUpright ^ players[0].visible) {
					if (isUpright) sc.PlayOneShot(sensor);
					flush(isUpright);
				}
				yield return null;
			}
		}
	}

	public void turn() {
		nextTurn();
		//GetComponent<NetworkView>().RPC("CurrentTurn", RPCMode.All, (current+1)%cnt);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Collider2D[] col = {};
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				if (col[0].gameObject.transform.GetChild(0).name == "value") {
					deselectAll();
					StartCoroutine(cardMouseEvent());
				} else if ((playing ^ isTempDrawed()) && col[0].gameObject.name.Contains("pivot")) {
					StartCoroutine(envMouseEvent());
				}
			} else {
				deselectAll();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape)) { StartCoroutine(gameOver(false)); Application.LoadLevel(0); }
	}

	IEnumerator cardMouseEvent() {
		List<string> names = new List<string>();
		Collider2D[] col = {};
		while (Input.GetMouseButton(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				bool isCard = col[0].gameObject.transform.GetChild(0).name == "value";
				if (isCard) {
					Card card = col[0].gameObject.GetComponent<Card>();
					if (card != null && !names.Contains(card.name)) {
						names.Add(card.name);
						StartCoroutine(card.MouseEvent());
					}
				}
			}
			yield return new WaitForFixedUpdate();
		}
	}

	IEnumerator envMouseEvent() {
		List<string> names = new List<string>();
		Collider2D[] col = {};
		while (Input.GetMouseButton(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				bool isEnv = col[0].gameObject.name.Contains("pivot");
				if (isEnv) {
					CardEnv env = col[0].gameObject.GetComponent<CardEnv>();
					if (env != null && !names.Contains(env.name)) {
						names.Add(env.name);
						StartCoroutine(env.MouseEvent());
					}
				}
			}
			yield return new WaitForFixedUpdate();
		}
	}

	IEnumerator startMouseEvent() {
		Collider2D[] col = {};
		while(true) {
			if (Input.GetMouseButton(0)) {
				col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				if(col.Length > 0) {
					bool isEnv = col[0].gameObject.name.Contains("pivot");
					if (isEnv && Network.connections.Length > 0) {
						GetComponent<NetworkView>().RPC("StartGame", RPCMode.All);
						yield break;
					}
				}
			}
			yield return null;
		}
	}

	IEnumerator waitForDealing() {
		while (true) {
			if (players[0].getCount() >= 4) yield break;
			yield return null;
		}
	}

	public string getIP() {
		IPHostEntry iep = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress ip = iep.AddressList[0];
		return ip.ToString();
	}

	void OnPlayerConnected(NetworkPlayer p) {
		GetComponent<NetworkView>().RPC("ClientInit", p, Network.connections.Length);
		GetComponent<NetworkView>().RPC("PlayersCnt", RPCMode.All, Network.connections.Length+1);
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		StartCoroutine(gameOver(true));
	}

	void OnPlayerDisconnected(NetworkPlayer p) {
		GetComponent<NetworkView>().RPC("PlayersCnt", RPCMode.All, Network.connections.Length+1);
	}
		
	NetworkConnectionError connect(string ip) {
		if (isClient()) {

			return new NetworkConnectionError();
		}
		return Network.Connect(ip,port);
	}

	void serverInit() {
		StartCoroutine(startMouseEvent());
		num = 0;
		tName.text = "玩家" + num;
		if (isServer()) return;
		Network.InitializeServer(3, port, false);
		//MasterServer.RegisterHost("TheDavinciCode", ip);
	}

	bool isServer() {
		return Network.peerType == NetworkPeerType.Server;
	}

	bool isClient() {
		return Network.peerType == NetworkPeerType.Client;
	}

	void sendLength() {
		/*foreach (NetworkPlayer p in Network.connections) 
			networkView.RPC("ClientInit", p, Network.connections.Length);/**/
		GetComponent<NetworkView>().RPC("PlayersCnt", RPCMode.All, Network.connections.Length+1);
	}

	void nextTurn() {
		int t = current;
		while (true) {
			t = (t+1)%cnt;
			if (op.Contains(t)) continue;
			GetComponent<NetworkView>().RPC("CurrentTurn", RPCMode.All, t);
			break;
		}
	}
	
	public bool inGame(int p) {
		return players[p].hasPrivate();
	}


	public void checkInGame() {
		if (!inGame(0)) GetComponent<NetworkView>().RPC("PlayerOut", RPCMode.All, num);
	}

	public bool isPlaying() {
		return op.Count < cnt-1;
	}

	public int getWinner() {
		if (isPlaying()) return -1;	// Not Over Yet
		for (int i = 0; i < cnt; i++) if (!op.Contains(i)) return i;
		return -2; // Wtf: No winner?!
	}

	public void guessSucceed() {
		keyCard.setVisible(true);
		checkInGame();
	}

	public void nextGuess(CardPool player, bool isWhite) {
		Card card = player.removeTemp();
		if (card.getColor()) pool[0].add(card);
		else pool[1].add(card);
		player.add(pool[isWhite?0:1].getRandom());
	}

	public bool judge(bool keepGuessing) {
		GetComponent<NetworkView>().RPC("deselectAll", RPCMode.All);
		bool correct = keyCard.correct();
		GetComponent<NetworkView>().RPC("setText", RPCMode.All, "玩家" + current + "猜测错误");
		if (correct) {
			GetComponent<NetworkView>().RPC("setText", RPCMode.All, "玩家" + current + "猜测正确");
			guessSucceed();
		}
		GetComponent<NetworkView>().RPC("RRestore", RPCMode.All);
		if (correct && keepGuessing) GetComponent<NetworkView>().RPC("RRemoveTemp", RPCMode.Others);
		else GetComponent<NetworkView>().RPC("endGuess", RPCMode.Others, correct);
		return correct;
	}

	public IEnumerator gameOver() {
		int winner = getWinner();
		if (winner == num) sc.PlayOneShot(win);
		else sc.PlayOneShot(lose);
		info.text = "玩家" + winner + "获得胜利！";
		yield return new WaitForSeconds(3f);
		for (int i = 0; i < players.Length; i++) {
			List<Card> c = players[i].removeAll();
			if (c != null && c.Count > 0) {
				foreach (Card t in c) {
					if (t.color) pool[0].add(t);
					else pool[1].add(t);
				}
			}
		}
		gameStarted = false;
		yield return new WaitForSeconds(2f);
		Application.LoadLevel(Application.loadedLevel);
	}

	public void play(bool isPivot){
		if (isPivot) sc.PlayOneShot(pivot);
		else sc.PlayOneShot(draw);
	}

	void flush(bool visible) {
		players[0].flush(visible);
	}

	public List<int> getKnownNumbers(bool isWhite, int me) {
		List<int> known = new List<int>();
		for (int i = 0; i < players.Length; i++)
			known.AddRange(players[i].getKnownNumbers(isWhite, i==me));
		known.Sort();
		return known;
	}

	public static bool getPlaying() {
		return playing;
	}

	public bool isTempDrawed() {
		return tempDrawed || (pool[0].Count()==0 && pool[1].Count()==0);
	}

	public static void setKeyCard(Card keyCard) {
		keyCard.select();
		DaVinci.keyCard = keyCard;
	}

	IEnumerator gameOver(bool isUnexpected) {
		sc.PlayOneShot(lose);
		if (isUnexpected) DaVinci.info.text = "游戏结束(与服务器断开连接)";
		else DaVinci.info.text = "游戏结束！";
		for (int i = 0; i < players.Length; i++) {
			List<Card> c = players[i].removeAll();
			if (c != null && c.Count > 0) {
				foreach (Card t in c) {
					if (t.color) pool[0].add(t);
					else pool[1].add(t);
				}
			}
		}
		gameStarted = false;
		yield return new WaitForSeconds(5f);
		Application.LoadLevel(Application.loadedLevel);
	}

	public void RemoveFromEnv(int c, int v) {
		GetComponent<NetworkView>().RPC("RRemove", RPCMode.All, num, c, v);
		GetComponent<NetworkView>().RPC("setText", RPCMode.All, "玩家"+current+"抽取了一张"+(c==0?"白牌":"黑牌"));
		tempDrawed = true;
	}
	public void setTextToAll(string text) {
		GetComponent<NetworkView>().RPC("setText", RPCMode.All, text);
	}
	[RPC]
	void endGuess(bool succeeded) {
		if (current == num) players[0].insertTemp(!succeeded);
	}
	[RPC]
	void RRemoveTemp() {
		Card tc;
		if (current == num) tc = players[0].removeTemp();
		else { tc = cTemp; cTemp = null; }
		if (tc == null) return;
		if (tc.getColor())
			pool[0].add(tc);
		else 
			pool[1].add(tc);
		playing = false;
	}
	/*
	 * Only invoke this on server.
	 *
	[RPC]
	void REffect(bool succeeded, bool keepGuessing) {
		networkView.RPC("RRestore", RPCMode.All);
		if (succeeded && keepGuessing) {
			if (current == 0)
				RRemoveTemp();
			else
				networkView.RPC("RRemoveTemp", RPCMode.Others);
		} else {
			if (current == 0)
				endGuess(succeeded);
			else
				networkView.RPC("endGuess", RPCMode.Others, succeeded);
		}
	}/**/
	[RPC]
	public void deselectAll() {
		for (int i = 0; i < players.Length; i++)
			players[i].deselectAll();
	}
	[RPC]
	void setText(string text) {
		info.text = text;
	}
	[RPC]
	void RDraw(int num, int c, int v) {
		Card cd = pool[c].remove(v);
		if (DaVinci.num == num) {
			players[0].add(cd);
		}
	}
	[RPC]
	void RRestore() {
		tempDrawed = false;
	}
	[RPC]
	void RRemove(int num, int c, int v) {
		Card cd = pool[c].remove(v);
		if (DaVinci.num == num) {
			players[0].draw(cd);
		} else cTemp = cd;
		tempDrawed = true;
	}
	[RPC]
	void StartGame() {
		gameStarted = true;
		pool[0].flush();
		pool[1].flush();
		DaVinci.info.text = "游戏开始！";
		StartCoroutine(Sensor());
	}
	[RPC]
	void ClientInit(int num) {
		DaVinci.num = num;
		tName.text = "玩家" + num;
	}
	[RPC]
	void PlayersCnt(int cnt) {
		DaVinci.cnt = cnt;
	}
	[RPC]
	void CurrentTurn(int current) {
		int c = DaVinci.current;
		DaVinci.current = current;
		tempDrawed = false;
		if (c == num) { playing = false; info.text = "轮到玩家" + current + "猜牌"; }
		if (current != num) info.text = "等待玩家" + current + "猜牌";
	}
	[RPC]
	public void PlayerOut(int o) {
		if (!op.Contains(o)) op.Add(o);
	}
}
