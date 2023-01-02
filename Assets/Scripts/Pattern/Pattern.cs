using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public abstract class Pattern : MonoBehaviour, IPattern,IEventHandler {

	#region IPattern implementation
	public abstract Enemy SpawnEnemy();
	public abstract float NextWaitDurationBeforeSpawn { get; }

	public bool IsPatternOver
	{
		get
		{
			return m_NSpawnedEnemies == m_NEnemiesToSpawn
			  && m_Enemies.Count == 0;
		}
	}

	public virtual void StartPattern()
	{
		StartCoroutine(SpawnCoroutine());
	}
	#endregion

	#region IEventHandler implementation
	public void SubscribeEvents()
	{
		EventManager.Instance.AddListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);
	}

	public void UnsubscribeEvents()
	{
		EventManager.Instance.RemoveListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);
	}
	#endregion

	#region Monobehaviour Implementation
	private void Awake()
	{
		SubscribeEvents();
	}
	private void OnDestroy()
	{
		UnsubscribeEvents();
	}
	#endregion

	[Header("Pattern")]
	#region enemies spawn
	[SerializeField] protected GameObject m_EnemyPrefab;
	[SerializeField] private int m_NEnemiesToSpawn;
	private int m_NSpawnedEnemies;

	[SerializeField] private float m_WaitDurationAfterLastEnemySpawn=1;
	List<Enemy> m_Enemies = new List<Enemy>();

	IEnumerator SpawnCoroutine()
	{
		m_NSpawnedEnemies = 0;

		while (m_NSpawnedEnemies< m_NEnemiesToSpawn)
		{
			float waitDurationBeforeSpawn = NextWaitDurationBeforeSpawn;
			float elapsedTime = 0;
			while(elapsedTime< waitDurationBeforeSpawn)
			{
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			m_NSpawnedEnemies++;
			Enemy newEnemy = SpawnEnemy();
			newEnemy.transform.SetParent(transform);
			m_Enemies.Add(newEnemy);
		}

		yield return new WaitForSeconds(m_WaitDurationAfterLastEnemySpawn);
		EventManager.Instance.Raise(new PatternHasFinishedSpawningEvent());
		yield break;
	}

	void ClearEnemies()
	{
		foreach (var item in m_Enemies)
		{
			if (item) Destroy(item.gameObject);
		}
		m_Enemies.Clear();
	}
	#endregion

	#region Callbacks to Enemy events
	void EnemyHasBeenDestroyed(EnemyHasBeenDestroyedEvent e)
	{
		m_Enemies.Remove(e.eEnemy);
		if (IsPatternOver)
		{
			EventManager.Instance.Raise(new AllEnemiesOfPatternHaveBeenDestroyedEvent() { ePattern = this });
		}
	}
	#endregion
}
