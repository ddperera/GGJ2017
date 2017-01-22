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
		initialColor = GetComponent<Renderer>().material.GetColor("_Color");
	}

	override public void OnClick()
	{
		SetClicked();
	}
	
	public void SetClicked()
	{
		hasBeenClicked = true;
		StartCoroutine(FadeOut());
	}

	public void SetUnclicked()
	{
		hasBeenClicked = false;
		GetComponent<Renderer>().material.SetColor("_Color", initialColor);
		GetComponent<Renderer>().material.SetFloat("_FadeAmount", 0f);
	}

	private IEnumerator FadeOut()
	{
		Color c = initialColor;
		for (float t=0f; t<1f; t += Time.deltaTime / .5f)
		{
			yield return null;
			c = Color.Lerp(initialColor, clickColor, t);
			GetComponent<Renderer>().material.SetColor("_Color", c);
			GetComponent<Renderer>().material.SetFloat("_FadeAmount", t);
		}
		c = clickColor;
		GetComponent<Renderer>().material.SetColor("_Color", c);
		GetComponent<Renderer>().material.SetFloat("_FadeAmount", 1f);
	}
}
