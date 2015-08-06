using UnityEngine;
using System.Collections;

public class CardValueS : MonoBehaviour {

	CardS parent;

	// Use this for initialization
	void Start () {
		parent = transform.parent.gameObject.GetComponent<CardS>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void flush() {
		if (parent.player!=null && parent.player.name == "player" && !parent.getVisible()) {
			setTranslucent();
			transform.position = new Vector3(transform.position.x, 
				transform.position.y, parent.transform.position.z - 0.5f);
			return;
		}
		setVisible(parent.getVisible());
		transform.position = new Vector3(transform.position.x, 
			transform.position.y, parent.transform.position.z - 0.5f);
	}

	void setTranslucent() {
		Color c = GetComponent<Renderer>().material.color;
		GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 0.9f);
		c = parent.GetComponent<Renderer>().material.color;
		parent.GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 0.6f);
	}

	void setVisible(bool visible) {
		Color vc = GetComponent<Renderer>().material.color, pc = parent.GetComponent<Renderer>().material.color;
		if (visible) {
			GetComponent<Renderer>().material.color = new Color(vc.r, vc.g, vc.b, 1f);
			parent.GetComponent<Renderer>().material.color = new Color(pc.r, pc.g, pc.b, 1f);
		} else {
			GetComponent<Renderer>().material.color = new Color(vc.r, vc.g, vc.b, 0f);
		}
	}
}
