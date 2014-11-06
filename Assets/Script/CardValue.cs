using UnityEngine;
using System.Collections;

public class CardValue : MonoBehaviour {

	Card parent;

	// Use this for initialization
	void Start () {
		parent = transform.parent.gameObject.GetComponent<Card>();
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
		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, 0.9f);
		c = parent.renderer.material.color;
		parent.renderer.material.color = new Color(c.r, c.g, c.b, 0.6f);
	}

	void setVisible(bool visible) {
		Color vc = renderer.material.color, pc = parent.renderer.material.color;
		if (visible) {
			renderer.material.color = new Color(vc.r, vc.g, vc.b, 1f);
			parent.renderer.material.color = new Color(pc.r, pc.g, pc.b, 1f);
		} else {
			renderer.material.color = new Color(vc.r, vc.g, vc.b, 0f);
		}
	}
}
