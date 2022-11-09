using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;
using SDD.Events;

public class EnemiesManager : Manager<EnemiesManager> {

	[Header("EnemiesManager")]
	#region patterns & current pattern management
	[SerializeField] GameObject[] m_PatternsPrefabs;
	private int m_CurrentPatternIndex;
	private GameObject m_CurrentPatternGO;
	private IPattern m_CurrentPattern;
	public IPattern CurrentPattern { get { return m_CurrentPattern; } }
	#endregion

	#region Events' subscription
	public override void SubscribeEvents()
	{
		base.SubscribeEvents();

		EventManager.Instance.AddListener<PatternHasFinishedSpawningEvent>(PatternHasFinishedSpawning);
		EventManager.Instance.AddListener<AllEnemiesOfPatternHaveBeenDestroyedEvent>(AllEnemiesOfPatternHaveBeenDestroyed);
		EventManager.Instance.AddListener<GoToNextPatternEvent>(GoToNextPattern);
	}

	public override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();

		EventManager.Instance.RemoveListener<PatternHasFinishedSpawningEvent>(PatternHasFinishedSpawning);
		EventManager.Instance.RemoveListener<AllEnemiesOfPatternHaveBeenDestroyedEvent>(AllEnemiesOfPatternHaveBeenDestroyed);
		EventManager.Instance.RemoveListener<GoToNextPatternEvent>(GoToNextPattern);
	}
	#endregion

	#region Manager Implementation
	protected override IEnumerator InitCoroutine()
	{
		yield break;
	}
	#endregion

	#region Pattern flow
	void Reset()
	{
		Destroy(m_CurrentPatternGO);
		m_CurrentPatternGO = null;
		m_CurrentPatternIndex = -1;
	}

	IPattern InstantiatePattern(int levelIndex)
	{
		levelIndex = Mathf.Max(levelIndex, 0) % m_PatternsPrefabs.Length;
		m_CurrentPatternGO = Instantiate(m_PatternsPrefabs[levelIndex]);
		return m_CurrentPatternGO.GetComponent<IPattern>();
	}

	private IEnumerator InstantiatePatternCoroutine()
	{
		Destroy(m_CurrentPatternGO);
		while (m_CurrentPatternGO) yield return null;

		m_CurrentPattern = InstantiatePattern(m_CurrentPatternIndex);
		m_CurrentPattern.StartPattern();

		EventManager.Instance.Raise(new PatternHasBeenInstantiatedEvent() { ePattern = m_CurrentPattern });
	}
	#endregion

	#region Callbacks to GameManager events
	protected override void GameMenu(GameMenuEvent e)
	{
		Reset();
	}
	protected override void GamePlay(GamePlayEvent e)
	{
		Reset();
		EventManager.Instance.Raise(new GoToNextPatternEvent());
	}
	#endregion

	#region Callbacks to EnemiesManager events
	public void GoToNextPattern(GoToNextPatternEvent e)
	{
		m_CurrentPatternIndex++;
		StartCoroutine(InstantiatePatternCoroutine());
	}
	#endregion

	#region Callbacks to Pattern events
	void AllEnemiesOfPatternHaveBeenDestroyed(AllEnemiesOfPatternHaveBeenDestroyedEvent e)
	{
		EventManager.Instance.Raise(new GoToNextPatternEvent());
	}
	void PatternHasFinishedSpawning(PatternHasFinishedSpawningEvent e)
	{
	}
	#endregion
}