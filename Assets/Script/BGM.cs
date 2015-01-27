using UnityEngine;
using System.Collections;

public class BGM : MonoBehaviour {

	public AudioClip bgClip;
	private static AudioSource bg = null;
	/*private static BGM instance;  
	
	public static BGM GetInstance() {
		if (!instance) {
			instance = (BGM) GameObject.FindObjectOfType(typeof(BGM));
			if (!instance) 
				Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");  
		}
		return instance;
	}*/

	// Use this for initialization
	void Start () {
		if (bg == null) {
			bg = gameObject.AddComponent<AudioSource>();
			bg.clip = bgClip;
			bg.loop = true;
			bg.volume = 0.5F;
			bg.Play();
			DontDestroyOnLoad(this);
		}
	}
}
