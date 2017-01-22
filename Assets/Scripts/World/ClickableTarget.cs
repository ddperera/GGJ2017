using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTarget : Clickable {

	public float timePenalty;
	public Color clickColor;
	[HideInInspector]
	public bool hasBeenClicked;

	override public void OnClick()
	{
		hasBeenClicked = true;
		GetComponent<Renderer>().material.color = clickColor;
	}
}
