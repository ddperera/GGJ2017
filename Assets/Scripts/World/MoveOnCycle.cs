﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnCycle : TimeCycleListener {

	public Transform endLocationMarker;
	private Vector3 m_initialPosition;
	private bool m_atStart = true;

	void Start()
	{
		m_canCycle = true;
		m_curCycleCount = 0;

		m_initialPosition = transform.position;
	}

	public override void OnCycle()
	{
		m_curCycleCount = m_curCycleCount + 1 % cycleActionFrequency;
		if (m_canCycle && m_curCycleCount == 0)
		{
			StartCoroutine(Move());
		}
	}

	private IEnumerator Move()
	{
		m_canCycle = false;

		for (float t=0f; t<1f; t += Time.deltaTime / cycleActionTime)
		{
			if (!m_atStart)
			{
				t = 1f - t;
			}
			transform.position = Vector3.Lerp(m_initialPosition, endLocationMarker.transform.position, t);
			yield return null;
		}
		transform.position = m_atStart ? endLocationMarker.transform.position : m_initialPosition;
		m_atStart = !m_atStart;

		foreach (ParticleSystem p in cycleFinishedEmitters)
		{
			p.Play(true);
		}

		m_canCycle = true;
	}
}
