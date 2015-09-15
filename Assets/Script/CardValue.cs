using UnityEngine;
using System.Collections;

public class CardValue : MonoBehaviour {

	Card parent;
	SpriteRenderer sr;
	Color vc, pc;
	Vector3 v;

	// Use this for initialization
	void Start () {
		parent = transform.parent.gameObject.GetComponent<Card>();
		vc = GetComponent<Renderer>().material.color;
		pc = parent.GetComponent<Renderer>().material.color;
		v = new Vector3(transform.position.x, 
		                transform.position.y,  parent.transform.position.z - 0.5f);
		sr = parent.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void flush() {
		/*if (parent.player!=null && parent.player.name == "player" && !parent.getVisible()) {
			setTranslucent();
			transform.position = new Vector3(transform.position.x, 
				transform.position.y, parent.transform.position.z - 0.5f);
			return;
		}/**/
		v.x = transform.position.x;
		v.y = transform.position.y;
		v.z = parent.transform.position.z - 0.5f;
		transform.position = v;
	}

	public void flush(bool visible) {
		setVisible(visible?true:parent.getVisible());
		flush();
	}

	void setTranslucent() {
		vc.a = 0.9f;
		GetComponent<Renderer>().material.color = vc;
		pc.a = 0.6f;
		parent.GetComponent<Renderer>().material.color = pc;
	}

	void setVisible(bool visible) {
		if (visible) {
			vc.a = 1f;
			GetComponent<Renderer>().material.color = vc;
			sr.sprite = parent.color?DaVinci.WH:DaVinci.BH;
		} else {
			vc.a = 0f;
			GetComponent<Renderer>().material.color = vc;
			sr.sprite = parent.color?DaVinci.WT:DaVinci.BT;
		}
	}
}
