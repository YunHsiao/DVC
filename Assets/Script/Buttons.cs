using UnityEngine;
using System.Collections;

public class Buttons : MonoBehaviour {

	public AudioClip pivot;
	private AudioSource sc = null;
	public GameObject[] buttons;
	public Sprite[] pressed;
	SpriteRenderer[] sr = new SpriteRenderer[3];
	Collider2D[] col;
	Color[] c = new Color[4];

	// Use this for initialization
	void Start () {
		sc = gameObject.AddComponent<AudioSource>();
		sc.clip = pivot;
		float scale = ((float)Screen.width / Screen.height) / (480f / 800f);
		if (scale < 1 && scale > 0)
			Camera.main.orthographicSize /= scale;
		sr[0] = buttons[0].GetComponent<SpriteRenderer>();
		sr[1] = buttons[1].GetComponent<SpriteRenderer>();
		sr[2] = buttons[2].GetComponent<SpriteRenderer>();
		/*for (int i = 0; i < buttons.Length; i++) {
			c[i] = buttons[i].renderer.material.color;
			c[i].a = 0f;
			buttons[i].renderer.material.color = c[i];
			c[i].a = 1f;
		}
		StartCoroutine(ColorLerp());/**/
	}

	IEnumerator ColorLerp() {
		yield return new WaitForSeconds(2f);
		while (buttons[3].renderer.material.color.a.ToString("0.00") != c[3].a.ToString("0.00")) {
			for (int i = 0; i < buttons.Length; i++) {
				c[i].a += 0.01f;
				buttons[i].renderer.material.color = c[i];
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length != 0) {
				sc.PlayOneShot(pivot);
				if(col[0].name == "btn_single") {
					sr[0].sprite = pressed[0];
				} else if (col[0].name == "btn_multi") {
					sr[1].sprite = pressed[1];
				} else if (col[0].name == "btn_exit") {
					sr[2].sprite = pressed[2];
				}
			} else {
				sr[0].sprite = pressed[3];
				sr[1].sprite = pressed[4];
				sr[2].sprite = pressed[5];
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length != 0) {
				if(col[0].name == "btn_single") {
					Application.LoadLevel(1);
				} else if (col[0].name == "btn_multi") {
					Application.LoadLevel(2);
				} else if (col[0].name == "btn_exit") {
					Application.Quit();
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
	}

}
