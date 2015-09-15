using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DaVinciS : MonoBehaviour {

	public CardPoolS[] players;
	public CardEnvS[] pool;
	public static CardS keyCard;
	public AI ai;
	public GUIText tip;
	public static GUIText info;
	public AudioClip win, lose, pivot, draw;
	private AudioSource sc = null;
	bool waiting;
	List<CardPoolS> op;

	// Use this for initialization
	void Start () {
		info = tip;
		op = new List<CardPoolS>();
		sc = gameObject.AddComponent<AudioSource>();
		sc.clip = lose;
		StartCoroutine(deal());
	}

	IEnumerator deal() {
		DaVinciS.info.text = "发牌...";
		for (int i = 0; i < 4; i++) {/**/
			for (int j = 0; j < players.Length; j++) {
				yield return new WaitForSeconds(0.03f);
				players[j].add(pool[Random.Range(0,2)%2==0?0:1].getRandom());
			}/*
			players[0].add(pool[i%2==0?0:1].getRandom());
			players[1].add(pool[i<2?0:1].getRandom());
			players[2].add(pool[i>1?0:1].getRandom());/**/
		}
		yield return new WaitForSeconds(0.5f);
		DaVinciS.info.text = "整牌...";
		players[0].sort();
		players[1].sort();
		players[2].sort();
		yield return new WaitForSeconds(1f);
		waiting = true;
		while (isPlaying()) {
			if (pool[0].Count()==0 && pool[1].Count()==0) 
				DaVinciS.info.text = "轮到玩家猜牌（已无新牌可取）";
			else DaVinciS.info.text = "轮到玩家猜牌";
			while (waiting) { 
				if (!isPlaying()) break;
				yield return null;
			}
			if (isPlaying()) yield return StartCoroutine(ai.move());
			//while (!waiting) yield return new WaitForFixedUpdate();
		}
		DaVinciS.info.text = "游戏结束！";
		StartCoroutine(gameOver());
	}

	public void turn() {
		waiting = !waiting;
	}
	
	// Update is called once per frame
	void Update () {
		if (waiting && Input.GetMouseButtonDown(0)) {
			Collider2D[] col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				if (col[0].gameObject.transform.GetChild(0).name == "value") {//GetComponent<Card>() != null) {
					deselectAll();
					StartCoroutine(cardMouseEvent());
				} else if (col[0].gameObject.name.Contains("pivot")) {//GetComponent<CardEnv>() != null) {
					StartCoroutine(envMouseEvent());
				}
			} else {
				deselectAll();
			}
		}/**/
		if (Input.GetKeyDown(KeyCode.Escape)) Application.LoadLevel(0);
	}

	IEnumerator cardMouseEvent() {
		List<string> names = new List<string>();
		Collider2D[] col;
		while (Input.GetMouseButton(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				bool isCard = col[0].gameObject.transform.GetChild(0).name == "value";
				if (isCard) {
					CardS card = col[0].gameObject.GetComponent<CardS>();
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
		Collider2D[] col;
		while (Input.GetMouseButton(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				bool isEnv = col[0].gameObject.name.Contains("pivot");
				if (isEnv) {
					CardEnvS env = col[0].gameObject.GetComponent<CardEnvS>();
					if (env != null && !names.Contains(env.name)) {
						names.Add(env.name);
						StartCoroutine(env.MouseEvent());
					}
				}
			}
			yield return new WaitForFixedUpdate();
		}
	}
	
	public bool inGame(int p) {
		return players[p].hasPrivate();
	}

	public bool isPlaying() {
		return op.Count < 2;
		/*int cnt = 0;
		foreach (CardPoolS p in players) {
			if (p.isPlaying()) cnt++;
		}
		if (cnt > 1) return true;
		return false;/**/
	}

	public int getWinner() {
		for (int i = 0; i < players.Length; i++) {
			if (players[i].hasPrivate()) return i;
		}
		return -1;
	}

	public void deselectAll() {
		for (int i = 0; i < players.Length; i++)
			players[i].deselectAll();
	}
	
	public void guessSucceed() {
		keyCard.setVisible(true);
		keyCard.player.flushCntV();
	}

	public void guessFail(CardPoolS player) {
		player.insertTemp(true);
	}

	public void nextGuess(CardPoolS player, bool isWhite) {
		CardS card = player.removeTemp();
		if (card.getColor()) pool[0].add(card);
		else pool[1].add(card);
		player.add(pool[isWhite?0:1].getRandom());
	}

	public void endGuess(CardPoolS player) {
		player.insertTemp(false);
		player.flushCntV();
	}

	public bool judge(CardPoolS player, bool keepGuessing) {
		deselectAll();
		if (keyCard.correct()) {
			DaVinciS.info.text = player.name + "猜测正确";
			guessSucceed();
			if (keepGuessing) {
				CardS tc = player.removeTemp();
				if (tc == null) return true;
				if (tc.getColor())
					pool[0].add(tc);
				else 
					pool[1].add(tc);
			} else {
				endGuess(player);
			}
			return true;
		} else {
			DaVinciS.info.text = player.name + "猜测错误";
			guessFail(player);
		}
		return false;
	}

	public IEnumerator gameOver() {
		int winner = getWinner();
		if (winner == 0 || winner == -1) {
			sc.PlayOneShot(win);
			DaVinciS.info.text = "玩家获得胜利！";
		} else {
			sc.PlayOneShot(lose);
			DaVinciS.info.text = players[winner].name + "获得胜利！";
		}
		revealAll();
		yield return new WaitForSeconds(4f);
		for (int i = 0; i < players.Length; i++) {
			List<CardS> c = players[i].removeAll();
			if (c != null && c.Count > 0) {
				foreach (CardS t in c) {
					if (t.color) pool[0].add(t);
					else pool[1].add(t);
				}
			}
		}
		yield return new WaitForSeconds(1f);
		Application.LoadLevel(Application.loadedLevel);
	}

	public void revealAll() {
		foreach (CardPoolS p in players) p.gameOver();
	}

	public List<int> getKnownNumbers(bool isWhite, int me) {
		List<int> known = new List<int>();
		for (int i = 0; i < players.Length; i++)
			known.AddRange(players[i].getKnownNumbers(isWhite, i==me));
		known.Sort();
		return known;
	}

	public static void setKeyCard(CardS keyCard) {
		keyCard.select();
		DaVinciS.keyCard = keyCard;
	}

	public void play(bool isPivot){
		if (isPivot) sc.PlayOneShot(pivot);
		else sc.PlayOneShot(draw);
	}

	public void playerOut(CardPoolS p) {
		if (!op.Contains(p)) op.Add(p);
	}
}
