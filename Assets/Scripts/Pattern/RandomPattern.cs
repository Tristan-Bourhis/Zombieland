using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomPattern : Pattern
{
	[Header("Random Pattern")]
	[SerializeField] private float m_MaxEnemySpawnPeriod;
	[SerializeField] private float m_MinEnemySpawnPeriod;
	[SerializeField] private AnimationCurve m_SpawnPeriodProbaCurve;
	[SerializeField] private float m_SpawnViewportYAmplitude;

	Vector3 m_PreviousSpawnPos;

	public override float NextWaitDurationBeforeSpawn {
		get { return Mathf.Lerp(m_MinEnemySpawnPeriod, m_MaxEnemySpawnPeriod, m_SpawnPeriodProbaCurve.Evaluate(Random.value)); }
	}

	private Vector3 RandomSpawnWorldPos { get { return Camera.main.ViewportToWorldPoint(new Vector3(1.025f, (Random.value - .5f) * m_SpawnViewportYAmplitude / 2f + .5f, -Camera.main.transform.position.z)); } }

	public override Enemy SpawnEnemy()
	{
		GameObject enemyGO = Instantiate(m_EnemyPrefab);
		Vector3 newSpawnWorldPos = Vector3.zero;

		do
		{
			newSpawnWorldPos = RandomSpawnWorldPos;
		} while (Vector3.Distance(m_PreviousSpawnPos, newSpawnWorldPos) < 2);

		enemyGO.transform.position = newSpawnWorldPos;
		m_PreviousSpawnPos = newSpawnWorldPos;

		Enemy enemy = enemyGO.GetComponent<Enemy>();
		return enemy;
	}
}
