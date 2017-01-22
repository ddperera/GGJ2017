using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialManagerBehaviour : MonoBehaviour {

    float timer;
    bool timeTicking;
	// Use this for initialization
	void Start ()
    {
        timer = 0f;
        timeTicking = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(timeTicking)
        {
            timer += Time.deltaTime;
        }
	}

    private void OnGUI()
    {

    }

    public void StartTime()
    {
        timeTicking = true;
    }

    public void StopTime()
    {
        timeTicking = false;
    }
}
