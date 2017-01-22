using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialEndTrigger : MonoBehaviour {

    public TimeTrialManagerBehaviour manager;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        manager.StopTime();
    }
}

