using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeCycleListener : MonoBehaviour {

	public float cycleActionTime;
	public float cycleActionFrequency;
	public ParticleSystem[] cycleFinishedEmitters;
	protected float m_curCycleCount;
	protected bool m_canCycle;

	public abstract void OnCycle();
}
