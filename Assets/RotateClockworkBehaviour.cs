using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateClockworkBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        StartCoroutine("ClockRotateNinety");
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    IEnumerator ClockRotateNinety()
    {
        while (true)
        {

            Vector3 initialRot = transform.rotation.eulerAngles;
            Vector3 targetRot = new Vector3(initialRot.x, initialRot.y, initialRot.z);
            targetRot.z += 90;
            targetRot.z %= 360;

            Vector3 currentRot = new Vector3(initialRot.x, initialRot.y, initialRot.z);

            for (float i = 0; i < 1; i += Time.deltaTime / 2.0f)
            {
                currentRot = Vector3.Lerp(initialRot, targetRot, i);
                transform.localEulerAngles = currentRot;
                yield return null;
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

}
