using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeTrialManagerBehaviour : MonoBehaviour {

    float timer;
    bool timeTicking;

    float timeStart;
    float timeEnd;

    TimeSpan t;
     
    public Text timerText;

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
            timer = Time.time - timeStart;
            t = TimeSpan.FromSeconds(timer);
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D3}",
                t.Minutes,
                t.Seconds,
                t.Milliseconds);
        }
	}

    public void StartTime()
    {
        timeTicking = true;
        timeStart = Time.time;
        GameObject[] clickables = GameObject.FindGameObjectsWithTag("Clickable");

        for (int i = 0; i < clickables.Length; i++)
        {
            clickables[i].GetComponent<ClickableTarget>().SetUnclicked();
        }
    }

    public void StopTime()
    {
        timeEnd = Time.time;
        timeTicking = false;
        GameObject[] clickables = GameObject.FindGameObjectsWithTag("Clickable");
        int unclickedCounter = 0;
        float unclickedTimeAdded = 0f;
        for (int i = 0; i < clickables.Length; i++)
        {
            if (clickables[i].GetComponent<ClickableTarget>().hasBeenClicked == false)
            {
                unclickedCounter++;
                unclickedTimeAdded += clickables[i].GetComponent<ClickableTarget>().timePenalty;
            }
        }

        TimeSpan tBeforeEnemies = TimeSpan.FromSeconds(timeEnd - timeStart);
        TimeSpan tAfterEnemies = TimeSpan.FromSeconds((timeEnd - timeStart) + unclickedTimeAdded);
        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D3} + {3} missed targets * {4} = {5:D2}:{6:D2}:{7:D3}",
                tBeforeEnemies.Minutes,
                tBeforeEnemies.Seconds,
                tBeforeEnemies.Milliseconds,
                unclickedCounter,
                clickables[0].GetComponent<ClickableTarget>().timePenalty,
                tAfterEnemies.Minutes,
                tAfterEnemies.Seconds,
                tAfterEnemies.Milliseconds);
    }
}
