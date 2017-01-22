using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingController : MonoBehaviour {

	void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f)), out hitInfo))
			{
				Clickable clickable = hitInfo.collider.GetComponent<Clickable>();
				if (clickable != null)
				{
					clickable.OnClick();
				}
			}
		}
	}

}
