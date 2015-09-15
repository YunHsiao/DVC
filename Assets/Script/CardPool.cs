using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardPool : MonoBehaviour {

	List<Card> cards;
	Card temp;
	public float max_range;
	public bool visible;

	// Use this for initialization
	void Start () {
		cards = new List<Card>();
		visible = true;
	}
	
	// Update is called once per frame
	void Update () {
	}

	public bool add(Card card) {
		//Debug.Log(card.name);
		if (card == null) return false;
		cards.Add(card);
		card.player = this;
		card.scale = transform.localScale.x;
		card.default_x = transform.position.x;
		card.y = transform.position.y;
		card.default_y = transform.position.y;
		for (int i = 0; i < cards.Count; i++) {
			cards[i].position = i;
			/*cards[i].SetLayer(-i);
			Vector3 p = cards[i].transform.GetChild(0).transform.position;
			cards[i].transform.GetChild(0).transform.position = 
				new Vector3(p.x, p.y, cards[i].transform.position.z-0.5f);/**/
			cards[i].SetPosition();
		}
		return true;
	}

	public void sort() {
		cards.Sort();
		for (int i = 0; i < cards.Count; i++) {
			cards[i].position = i;
			//if (transform.position.x > 0) cards[i].SetLayer(i);
			cards[i].SetZ(i,true);
			/*Vector3 p = cards[i].transform.GetChild(0).transform.position;
			cards[i].transform.GetChild(0).transform.position = 
				new Vector3(p.x, p.y, -i-0.5f);/**/
			cards[i].SetPosition();
		}
	}

	public void insert(Card card) {
		cards.Add(card);
		sort();
	}

	public void draw(Card card) {
		if (card == null) return;
		temp = card;
		card.player = this;
		card.default_x = transform.position.x;
		card.y = transform.position.y;
		card.default_y = transform.position.y;
		card.position = cards.Count+1;
		card.moveForTemp();
		card.scale = transform.localScale.x;
		for (int i = 0; i < cards.Count; i++) {
			cards[i].moveForTemp();
		}
	}

	public void insertTemp(bool visible) {
		if (temp != null) {
			temp.setVisible(visible);
			insert(temp);
			temp = null;
		}
	}

	public Card removeTemp() {
		Card t = temp;
		temp = null;
		sort();
		return t;
	}

	public List<Card> removeAll() {
		insertTemp(false);
		List<Card> c = cards;
		cards = null;
		return c;
	}

	public int getCount() {
		return cards.Count;
	}

	public List<Card> getCards() {
		return cards;
	}

	public void deselectAll () {
		if (cards == null) return;
		for (int i = 0; i < cards.Count; i++) {
			cards[i].deselect();
		}
		if (temp != null) temp.deselect();
	}

	public Card getCard(int index) {
		if (index < cards.Count)
			return cards.ToArray()[index];
		return null;
	}

	public int getSize() {
		return cards.Count;
	}

	public bool hasTemp() { 
		return temp != null;
	}

	public int[] ColorCount() {
		int[] count = new int[2];
		foreach (Card c in cards) {
			if (c.getColor()) count[0]++;
			else count[1]++;
		}
		return count;
	}

	public int ColorCount(bool isWhite) {
		int count = 0;
		foreach (Card c in cards) {
			if (c.getColor() == isWhite) count++;
		}
		return count;
	}

	public int[] getVisibleCountByColor() {
		int[] count = new int[2];
		foreach (Card c in cards) {
			if (c.getVisible()) {
				if (c.getColor()) count[0]++;
				else count[1]++;
			}
		}
		return count;
	}

	public List<int> getKnownNumbers(bool isWhite, bool isSelf) {
		List<int> known = new List<int>();
		if (isSelf) {
			foreach (Card c in cards) {
				if (c.getColor() == isWhite) {
					known.Add(c.getValue());
				}
			}
			if (tempColor() == (isWhite?0:1))
				known.Add(tempValue());
		} else {
			foreach (Card c in cards) {
				if (c.getVisible() && c.getColor() == isWhite) {
					known.Add(c.getValue());
				}
			}
		}
		return known;
	}

	public int[] getLimits(int index, bool targetColor) {
		int[] limits = {0,14};
		if (index > 0) {
			for (int i = index-1; i >= 0; i--) {
				Card t = getCard(i);
				if (t.getVisible()) {
					if (t.getColor() == targetColor)
						limits[0] = t.getValue()+1;
					else
						limits[0] = t.getValue();
					break;
				}
			}
		}
		if (index < cards.Count-1) {
			for (int i = index+1; i < cards.Count; i++) {
				Card t = getCard(i);
				if (t.getVisible()) {
					if (t.getColor() == targetColor)
						limits[1] = t.getValue();
					else
						limits[1] = t.getValue()+1;
					break;
				}
			}
		}
		return limits;
	}

	public int tempColor() {
		if (temp != null)
			return temp.getColor()?0:1;
		return -1;
	}

	public bool hasColor(bool isWhite) {
		foreach (Card c in cards) {
			if (!c.getVisible() && c.getColor() == isWhite) return true;
		}
		return false;
	}

	public bool hasPrivate() {
		foreach (Card c in cards) {
			if (!c.getVisible()) return true;
		}
		return false;
	}

	public int tempValue() {
		if (temp != null)
			return temp.getValue();
		return -1;
	}

	public void flush(bool visible) {
		this.visible = visible;
		foreach (Card c in cards) {
			c.getCV().flush(visible);
		}
		if (temp != null) temp.getCV().flush(visible);
	}
}
