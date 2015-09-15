using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {

	Collider2D[] col;
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp(0)) {
			col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if(col.Length != 0) {
				if (col[0].name == "logo") {
					Application.LoadLevel(0);
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape)) Application.LoadLevel(0);
	}
	
}
