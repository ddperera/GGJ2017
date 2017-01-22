using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicManager : MonoBehaviour {

	public AudioClip music1;
	public AudioClip tick1;
	public AudioClip boom1;

	public AudioSource music;
	public AudioSource tick;
	public AudioSource boom;

	public GameObject musicObject;
	public GameObject tickObject;
	public GameObject boomObject;

	public bool startMusic;

	public Vector3 location;

	// Use this for initialization
	void Start () {
		startMusic = true;
		music = musicObject.GetComponent<AudioSource> ();
		tick = tickObject.GetComponent<AudioSource> ();
		boom = boomObject.GetComponent<AudioSource> ();
		music.volume = 0f;
	}
	IEnumerator playMusic(){
		startMusic = false;
		music.PlayOneShot(music1);
		tick.PlayOneShot (tick1);
		boom.PlayOneShot (boom1);

		yield return new WaitForSeconds(124f);
		startMusic = true;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (startMusic == true) {
			StartCoroutine (playMusic ());
		}
	}
}
