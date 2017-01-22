using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour {

    private Vector3 respawnLoc;
    private Quaternion respawnRot;

	// Use this for initialization
	void Start () {
        respawnLoc= gameObject.transform.position;
        respawnRot = gameObject.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetRespawn(Vector3 position, Quaternion rotation)
    {
        respawnLoc = position;
        respawnRot = rotation;

        Debug.Log(respawnLoc);
    }

    public void Respawn()
    {
        gameObject.transform.position = respawnLoc;
        gameObject.transform.rotation = respawnRot;
    }
}

