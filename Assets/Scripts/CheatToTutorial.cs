using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatToTutorial : MonoBehaviour {
	public bool canCheat = false;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P) && canCheat)
		{
			GameObject.FindWithTag("Player").transform.position = gameObject.transform.position;
			canCheat = false;
		}
	}
}
