using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardEnv : MonoBehaviour {

	public Card[] cards;
	public CardEnv aPool;
	public DaVinci game;
	List<Card> list;
	public bool isWhite;
	//Collider2D[] col = {};

	// Use this for initialization
	void Start () {
		list = new List<Card>();
		list.AddRange(cards);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public IEnumerator MouseEvent() {
		if (game.isTempDrawed() && !DaVinci.getPlaying()) {
			game.play(true);
			if (isWhite) {
				if (DaVinci.keyCard != null){
					bool result = game.judge(true);
					yield return new WaitForSeconds(1);
					if (!result) game.turn();
				}
			} else {
				if (DaVinci.keyCard != null) {
					game.judge(false);
					yield return new WaitForSeconds(1);
					game.turn();
				}
			}
		} else {
			game.play(false);
			int v = getRandomNum();
			Card c = getCard(v);
			if (c != null) {
				c.scale = game.players[0].transform.localScale.x;
				Camera camera = Camera.main;
				if (camera) {
					//转换对象到当前屏幕位置
					Vector3 screenPosition = camera.WorldToScreenPoint (c.transform.position);
					//鼠标屏幕坐标
					Vector3 mScreenPosition;
					//若鼠标左键一直按着则循环继续
					while (Input.GetMouseButton (0)) {
						//鼠标屏幕上新位置
						mScreenPosition = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
						// 对象新坐标 
						c.Drag(camera.ScreenToWorldPoint (mScreenPosition));
						//协同，等待下一帧继续
						yield return new WaitForFixedUpdate ();
					}
					c.StopDraging();
					if (c.transform.position.y >= 0) {
						c.Drag(transform.position);
						c.scale = 0f;
					} else {
						game.RemoveFromEnv(isWhite?0:1, v);
					}
				}
			}
		}
	}

	public Card getRandom() {
		if (list.Count == 0) return null;
		int index = Random.Range(0, list.Count);
		Card card = (Card) list[index];
		list.RemoveAt(index);
		return card;
	}

	public Card remove(int index) {
		if (index >= 0 && index < list.Count) {
			Card card = (Card) list[index];
			list.RemoveAt(index);
			return card;
		}
		return null;
	}

	public Card getCard(int index) {
		if (index >= 0 && index < list.Count)
			return (Card) list[index];
		return null;
	}

	public int getRandomNum() {
		return Random.Range(0, list.Count);
	}

	public void add(Card card) {
		if (card == null || containsCard(card)) return;
		list.Add(card);
		card.player = null;
		card.position = 0;
		card.scale = 0f;
		card.x = transform.position.x;
		card.y = transform.position.y;
		card.SetZ(0f,true);
		card.default_x = 0f;
		card.default_y = 0f;
		card.getCV().flush(false);
		card.setVisible(false);
	}

	public int Count() {
		return list.Count;
	}

	public void flush() {
		foreach (Card c in list) 
			c.getCV().flush(false);
	}

	public bool containsCard(Card card) {
		foreach (Card c in list) 
			if (c.Equals(card)) return true;
		return false;
	}

}
