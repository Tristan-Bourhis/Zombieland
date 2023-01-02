using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Random = UnityEngine.Random;
using SDD.Events;

[System.Serializable]
public class ParallelPatterns
{
	public List<GameObject> m_Patterns = new List<GameObject>();
}

public class EnemiesManager : Manager<EnemiesManager> {

	[Header("EnemiesManager")]
	#region patterns & current pattern management
	[SerializeField] List<ParallelPatterns> m_ParallelPatternsPrefabs = new List<ParallelPatterns>();
	private int m_CurrentParallelPatternIndex;
	private List<GameObject> m_CurrentPatternsGO = new List<GameObject>();
	private List<IPattern> m_CurrentPatterns = new List<IPattern>();
	
	#endregion

	#region Events' subscription
	public override void SubscribeEvents()
	{
		base.SubscribeEvents();

		EventManager.Instance.AddListener<PatternHasFinishedSpawningEvent>(PatternHasFinishedSpawning);
		EventManager.Instance.AddListener<AllEnemiesOfPatternHaveBeenDestroyedEvent>(AllEnemiesOfPatternHaveBeenDestroyed);
		EventManager.Instance.AddListener<GoToNextParallelPatternsEvent>(GoToNextParallelPatterns);
	}

	public override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();

		EventManager.Instance.RemoveListener<PatternHasFinishedSpawningEvent>(PatternHasFinishedSpawning);
		EventManager.Instance.RemoveListener<AllEnemiesOfPatternHaveBeenDestroyedEvent>(AllEnemiesOfPatternHaveBeenDestroyed);
		EventManager.Instance.RemoveListener<GoToNextParallelPatternsEvent>(GoToNextParallelPatterns);
	}
	#endregion

	#region Manager Implementation
	protected override IEnumerator InitCoroutine()
	{
		yield break;
	}
	#endregion

	#region Pattern flow

	void DestroyPatterns()
	{
		m_CurrentPatternsGO.ForEach(item => Destroy(item));
	}

	void Reset()
	{
		DestroyPatterns();
		m_CurrentPatternsGO = new List<GameObject>();
		m_CurrentParallelPatternIndex = -1;
	}

	List<IPattern> InstantiatePatterns(int levelIndex)
	{
		m_CurrentPatternsGO.Clear();

		levelIndex = Mathf.Max(levelIndex, 0) % m_ParallelPatternsPrefabs.Count;
		foreach(var item in m_ParallelPatternsPrefabs[levelIndex].m_Patterns)
			m_CurrentPatternsGO.Add(Instantiate(item));

		return m_CurrentPatternsGO.Select(item=>item.GetComponent<IPattern>()).ToList();
	}

	private IEnumerator InstantiateParallelPatternsCoroutine()
	{
		DestroyPatterns();
		bool allPatternsNull = false;
		do {
			allPatternsNull = true;
			foreach (var item in m_CurrentPatternsGO) if (item) allPatternsNull = false;
			Debug.Log("allPatternsNull = "+ allPatternsNull + "     ");
			yield return null;
		}
		while (!allPatternsNull) ;

		m_CurrentPatterns = InstantiatePatterns(m_CurrentParallelPatternIndex);
		foreach(var item in m_CurrentPatterns)  item.StartPattern();

		EventManager.Instance.Raise(new PatternsHaveBeenInstantiatedEvent() { ePatterns = m_CurrentPatterns });
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
		EventManager.Instance.Raise(new GoToNextParallelPatternsEvent());
	}
	#endregion

	#region Callbacks to EnemiesManager events
	public void GoToNextParallelPatterns(GoToNextParallelPatternsEvent e)
	{
		m_CurrentParallelPatternIndex++;
		StartCoroutine(InstantiateParallelPatternsCoroutine());
	}
	#endregion

	#region Callbacks to Pattern events
	void AllEnemiesOfPatternHaveBeenDestroyed(AllEnemiesOfPatternHaveBeenDestroyedEvent e)
	{
		m_CurrentPatterns.Remove(e.ePattern);
		if(m_CurrentPatterns.Count==0) EventManager.Instance.Raise(new GoToNextParallelPatternsEvent());
	}
	void PatternHasFinishedSpawning(PatternHasFinishedSpawningEvent e)
	{
	}
	#endregion
}