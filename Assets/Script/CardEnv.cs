using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardEnv : MonoBehaviour {

	public Card[] cards;
	public CardPool player;
	public CardEnv aPool;
	public DaVinci game;
	List<Card> list;
	public bool isWhite;
	Collider2D[] col;

	// Use this for initialization
	void Start () {
		list = new List<Card>();
		list.AddRange(cards);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public IEnumerator MouseEvent() {
		if (player.hasTemp() || (Count()==0 && aPool.Count()==0)) {
			if (isWhite) {
				if (DaVinci.keyCard != null){
					bool result = game.judge(player, true);
					yield return new WaitForSeconds(1);
					if (!result) game.turn();
					else DaVinci.info.text = "轮到玩家猜牌";
				}
			} else {
				if (DaVinci.keyCard != null) {
					game.judge(player, false);
					yield return new WaitForSeconds(1);
					game.turn();
				}
			}
		} else {
			Card c = getRandom();
			if (c != null) {
				c.scale = player.transform.localScale.x;
				c.getCV().flush();
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
						add(c);
						c.Drag(transform.position);
						c.scale = 0f;
					} else {
						player.draw(c);
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

	public void add(Card card) {
		if (card == null) return;
		list.Add(card);
		card.player = null;
		card.position = 0;
		card.scale = 0f;
		card.default_x = 0f;
		card.y = 0f;
		card.default_y = 0f;
		card.SetLayer(0f);
		card.setVisible(false);
		card.x = transform.position.x;
		card.getCV().flush();
	}

	public int Count() {
		return list.Count;
	}

}
