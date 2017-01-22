using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCycleManager : MonoBehaviour {

	public float secondsPerCycle;

	private float m_lastCycleTime;
	private List<TimeCycleListener> m_listeners;
	
	void Start () {
		m_lastCycleTime = Time.time;
		m_listeners = new List<TimeCycleListener>();
		GameObject[] objs = GameObject.FindGameObjectsWithTag("TimeCycle");
		foreach (GameObject obj in objs)
		{
			TimeCycleListener listener = obj.GetComponentInChildren<TimeCycleListener>();
			if (listener != null && !m_listeners.Contains(listener))
			{
				m_listeners.Add(listener);
			}
		}
	}
	
	void Update () {
		if (Time.time >= m_lastCycleTime + secondsPerCycle)
		{
			m_lastCycleTime = Time.time;
		}

		foreach (TimeCycleListener listener in m_listeners)
		{
			if (Time.time >= m_lastCycleTime + secondsPerCycle - listener.cycleActionTime)
			{
				if (listener.cycledRecently) continue;
				listener.OnCycle();
				StartCoroutine(BlockListener(listener));
				//Debug.Log(listener + " cycling");
			}
		}
	}

	private IEnumerator BlockListener(TimeCycleListener l)
	{
		l.cycledRecently = true;
		yield return new WaitForSeconds(l.cycleActionTime + 0.05f);
		l.cycledRecently = false;
	}
}
