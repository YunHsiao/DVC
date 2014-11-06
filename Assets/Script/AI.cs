using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI : MonoBehaviour {

	public DaVinci game;
	CardPool[] role;
	CardEnv[] pool;
	List<int> known;
	int i, o;

	// Use this for initialization
	void Start () {
		role = game.players;
		pool = game.pool;
		i = 1; o = 2;
		known = new List<int>();
	}

	public IEnumerator move() {
		int cnt = 0;
		while (true) {
			if (!game.inGame(i)) {
				takeTurn();
				if (!game.inGame(i)) {
					StartCoroutine(game.gameOver());
					yield break;
				}
				cnt++;
				if (cnt >= role.Length-1) break;
			}
			bool isWhite = decideColor(i,o);
			role[i].draw(pool[isWhite?0:1].getRandom());
			DaVinci.info.text = "轮到" + role[i].name + "猜牌";
			yield return new WaitForSeconds(1f);
			int p = decidePlayer(i, o, isWhite) ? 0 : o, 
				cindex = decideTarget(p, isWhite);
			if (cindex < 0) {
				isWhite = !isWhite;
				p = decidePlayer(i, o, isWhite) ? 0 : o;
				cindex = decideTarget(p, isWhite);
			}
			Card c = role[p].getCard(cindex);
			DaVinci.setKeyCard(c);
			int gs = decideGuess(p, cindex, isWhite);
			c.setI(gs);
			DaVinci.info.text = role[i].name + "目标" + role[p].name + "，猜测值：" + gs;
			yield return new WaitForSeconds(2f);
			if (!game.judge(role[i], true)) {
				takeTurn();
				if (!game.inGame(i)) {
					if (game.inGame(0)) game.turn();
					else StartCoroutine(game.gameOver());
					yield break;
				}
				cnt++;
				if (cnt >= role.Length-1) {
					yield return new WaitForSeconds(1);
					break;
				}
			} else if (!game.isPlaying()) {
				StartCoroutine(game.gameOver());
				yield break;
			}
			yield return new WaitForSeconds(1);
		}
		game.turn();
	}

	void takeTurn() {
		int t = i;
		i = o;
		o = t;
	}

	bool decideColor(int me, int yo) {
		int[] color = role[me].ColorCount();
		if (color[0] > color[1]) return true;
		if (color[0] < color[1]) return false;
		if (color[0] == color[1]) {
			int[] c1 = role[0].ColorCount(), c2 = role[yo].ColorCount();
			if (c1[0] + c2[0] > c1[1] + c2[1]) return true;
			if (c1[0] + c2[0] < c1[1] + c2[1]) return false;
		}
		bool isWhite = Random.Range(0, 2) == 0;
		known = game.getKnownNumbers(isWhite, i);
		if (known.Count >= 14) {
			isWhite = !isWhite;
		}
		return isWhite;
	}
	
	bool decidePlayer(int me, int yo, bool isWhite) {
		if (!game.inGame(yo) || (!role[yo].hasColor(isWhite) && role[0].hasColor(isWhite))) return true;
		if (!game.inGame(0) || (!role[0].hasColor(isWhite) && role[yo].hasColor(isWhite))) return false;
		int i = role[0].ColorCount(isWhite), o = role[yo].ColorCount(isWhite);
		if (i > o) return true;
		else if (i < o) return false;
		else return Random.Range(0, 2) == 0;
	}

	int decideTarget(int yo, bool isWhite) {
		int t;
		if (game.inGame(yo) && role[yo].hasColor(isWhite)) {
			while (true) {
				t = Random.Range(0, role[yo].getSize());
				if (!role[yo].getCard(t).getVisible() 
				    && role[yo].getCard(t).getColor() == isWhite) return t;
			}
		}
		return -1;
	}

	int decideGuess(int tgt, int index, bool isWhite) {
		known = game.getKnownNumbers(isWhite, i);
		/*int[] d = known.ToArray();
		for (int j = 0; j < d.Length; j++) {
			Debug.Log(role[i].name + ": " + d[j]);
		}/**/
		List<int> guess = new List<int>();
		for (int j = 0; j < 14; j++) {
			if (known.Contains(j)) continue;
			guess.Add(j);
		}
		int[] limits = role[tgt].getLimits(index, isWhite);
		while (true) {
			int gs = Random.Range(limits[0], limits[1]);
			if (guess.Contains(gs)) return gs;
		}
	}

}
