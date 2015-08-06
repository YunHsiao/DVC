using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

	public int value;
	public bool color;
	bool visible, isDraging; //, selected;
	int i;
	float speed;//, tx = 0f, ty = 0f, tsc = 0f;
	CardValue cv;
	Renderer rdr;
	Vector3 po, sc;
	public float x, y, z, scale = 0f;
	public float default_y = 0f, default_x = 0f, defalut_z = 0f;
	public CardPool player;
	public int position = 0;	//Position in Player's Hand

	// Use this for initialization
	void Start () {
		i = -1;
		visible = false;
		x = transform.position.x;
		y = transform.position.y;
		z = transform.position.z;
		po = new Vector3(0, 0, transform.position.z);
		sc = new Vector3(0, 0, 1);
		cv = GetComponentInChildren<CardValue>();
		rdr = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.x.ToString("0.00") != x.ToString("0.00")
		    || transform.position.y.ToString("0.00") != y.ToString("0.00")) {
			speed = 0.1f;
			if (isDraging) speed = 1f;
			po.x=x;po.y=y;po.z=z;
			transform.position = Vector3.Lerp(transform.position, 
				po, speed);
		}
		if (transform.localScale.x != scale) {
			speed = 0.1f;
			if (isDraging) speed = 1f;
			sc.x=scale;sc.y=scale;
			transform.localScale = Vector3.Lerp(transform.localScale, 
				sc, speed);
		}

		/*if (Input.GetMouseButtonDown(0)) {
			Collider2D[] col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0) {
				if (col[0].name == this.name) {

				}
			}
		}/**/
	}
		
	public void Drag(Vector3 position) {
		isDraging = true;
		this.x = position.x;
		this.y = position.y;
	}

	public void StopDraging() {
		isDraging = false;
	}

	public IEnumerator MouseEvent() {
		if (player == null) yield break;
		Collider2D[] col = {};
		while(Input.GetMouseButton(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length > 0 && col[0].name == name) {
				select();
			} else {
				deselect();
			}
			yield return null;
		}
		col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if(col.Length > 0 && col[0].name == name) {
			if (visible || DaVinci.getPlaying()) DaVinci.info.text = name;
			else {
				i += 1;
				if (i == 14) i = 0;
				DaVinci.info.text = "目标" + player.name + "，猜测值：" + i;
				DaVinci.setKeyCard(this);
			}
		}
	}

	public void select() {
		//selected = true;
		y = default_y + 1 * player.transform.localScale.x;
		SetZ(-9f, false);
		scale = player.transform.localScale.x + 0.1f;
		cv.flush(false);
	}
	
	public void deselect() {
		//selected = false;
		y = default_y;
		SetZ(defalut_z, false);
		scale = player.transform.localScale.x;
		cv.flush(false);
	}
	
	public void SetPosition() {
		float num_cards = player.getSize();
		float spacing = Mathf.Min(rdr.bounds.size.x 
		    	 / transform.localScale.x * player.transform.localScale.x * 0.95f,
		    player.max_range / num_cards);
		//Debug.Log(spacing + " " + name);
		this.x = default_x + position * spacing - (num_cards - 1) * spacing / 2.0f;
	}

	public void moveForTemp() {
		float num_cards = player.getSize()+2;
		float spacing = Mathf.Min(rdr.bounds.size.x 
				/ transform.localScale.x * player.transform.localScale.x * 0.95f,
			player.max_range / num_cards);
		this.x = default_x + position * spacing - (num_cards - 1) * spacing / 2.0f;
	}

	public void SetZ(float z, bool changeDefault) {
		//transform.position = new Vector3(transform.position.x, transform.position.y, z);
		this.z = z;
		if (changeDefault) defalut_z = z;
	}

	public void setI(int i) {
		this.i = i;
	}

	public void setVisible(bool visible) {
		this.visible = visible;
		cv.flush(false);
	}
	
	public void flip() {
		visible = !visible;
	}

	public bool getVisible() {
		return visible;
	}

	public bool getColor() {
		return color;
	}

	public int getValue() {
		return value;
	}

	public bool correct() {
		return i == value;
	}

	public CardValue getCV() {
		return cv;
	}

	public bool Equals(Card c) {
		if (!(c.getColor() ^ color) && c.getValue() == value) return true;
		return false;
	}
}

class CardComparer : IComparer<Card> {
	public int Compare(Card x, Card y) {
		int a = x.value, b = y.value;
		if (a > b) return 1;
		if (a < b) return -1;
		if (x.color) return 1;
		return -1;
	}
}
