using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTarget : Clickable {

	public float timePenalty;
	public Color clickColor;
	private Color initialColor;
	[HideInInspector]
	public bool hasBeenClicked = false;

	void Start()
	{
		initialColor = GetComponent<Renderer>().material.color;
	}

	override public void OnClick()
	{
		SetClicked();
	}
	
	public void SetClicked()
	{
		hasBeenClicked = true;
		GetComponent<Renderer>().material.color = clickColor;
	}

	public void SetUnclicked()
	{
		hasBeenClicked = false;
		GetComponent<Renderer>().material.color = initialColor;
	}
}
