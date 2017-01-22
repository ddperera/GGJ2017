using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioActivateTrigger : MonoBehaviour {

	public float fadeInTime;

	private bool hasTriggered = false;
	private musicManager music;

	// Use this for initialization
	void Start () {
		music = GameObject.FindWithTag("MusicManager").GetComponent<musicManager>();
	}

	void OnTriggerEnter(Collider other)
	{
		if (hasTriggered) return;
		hasTriggered = true;
		StartCoroutine(FadeInMusic());
	}

	private IEnumerator FadeInMusic()
	{
		for (float t=0f; t<1f; t += Time.deltaTime / fadeInTime)
		{
			music.music.volume = t/2f;
			yield return null;
		}
		music.music.volume = .5f;
	}
}
