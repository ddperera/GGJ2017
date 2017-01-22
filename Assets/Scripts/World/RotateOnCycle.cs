using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnCycle : TimeCycleListener {

	public Transform rotationRoot;
	public float rotationAmountPerCycle;

	void Start () {
		m_canCycle = true;
		m_curCycleCount = 0;
	}

	public override void OnCycle()
	{
		m_curCycleCount = m_curCycleCount + 1 % cycleActionFrequency;
		if (m_canCycle && m_curCycleCount == 0)
		{
			StartCoroutine(Rotate());
		}
	}

	private IEnumerator Rotate()
	{
		m_canCycle = false;
		float totalRot = 0f;

		for (float t = 0f; t < cycleActionTime; t += Time.deltaTime)
		{
			transform.RotateAround(rotationRoot.position, rotationRoot.forward, rotationAmountPerCycle / (Time.deltaTime * cycleActionTime));
			totalRot += rotationAmountPerCycle / (Time.deltaTime * cycleActionTime);
			yield return null;
		}
		transform.RotateAround(rotationRoot.position, rotationRoot.forward, rotationAmountPerCycle-totalRot);

		foreach (ParticleSystem p in cycleFinishedEmitters)
		{
			p.Play(true);
		}

		m_canCycle = true;
	}
}
