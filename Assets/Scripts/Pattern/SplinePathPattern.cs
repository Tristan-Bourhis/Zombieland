using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SplinePathPattern : Pattern
{
	[Header("Spline Path Pattern")]
	[SerializeField] private iTweenPath m_Path;

	[SerializeField] private float m_MaxEnemySpawnPeriod;
	[SerializeField] private float m_MinEnemySpawnPeriod;
	[SerializeField] private AnimationCurve m_SpawnPeriodProbaCurve;
	
	public override float NextWaitDurationBeforeSpawn {
		get { return Mathf.Lerp(m_MinEnemySpawnPeriod, m_MaxEnemySpawnPeriod, m_SpawnPeriodProbaCurve.Evaluate(Random.value)); }
	}

	public override Enemy SpawnEnemy()
	{
		GameObject enemyGO = Instantiate(m_EnemyPrefab);
		enemyGO.transform.position = m_Path.nodes[0];

		EnemySplinePathMvt enemy = enemyGO.GetComponent<EnemySplinePathMvt>();
		enemy.InitPath(m_Path);

		return enemy;
	}
}
