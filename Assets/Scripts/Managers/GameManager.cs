using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public enum GameState { gameMenu,gamePlay,gamePause,gameOver,gameVictory}

public class GameManager : Manager<GameManager> {

	//Game State
	private GameState m_GameState;
	public bool IsPlaying { get { return m_GameState == GameState.gamePlay; } }

	// TIME SCALE
	private float m_TimeScale;
	public float TimeScale { get { return m_TimeScale; } }
	void SetTimeScale(float newTimeScale)
	{
		m_TimeScale = newTimeScale;
		Time.timeScale = m_TimeScale;
	}

	//SCORE
	private int m_Score;
	public int Score {
		get { return m_Score; }
		set
		{
			m_Score = value;
			BestScore = Mathf.Max(BestScore, value);
		}
	}

	public int BestScore
	{
		get
		{
			return PlayerPrefs.GetInt("BEST_SCORE", 0);
		}
		set
		{
			PlayerPrefs.SetInt("BEST_SCORE", value);
		}
	}

	void IncrementScore(int increment)
	{
		SetScore(m_Score + increment);
	}

	void SetScore(int score)
	{
		Score = score;
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = m_Score, eNLives = m_NLives, eNEnemiesLeftBeforeVictory = m_NEnemiesLeftBeforeVictory });
	}

	//LIVES
	[Header("GameManager")]
	[SerializeField] private int m_NStartLives;

	private int m_NLives;
	public int NLives { get { return m_NLives; } }
	void DecrementNLives(int decrement)
	{
		SetNLives(m_NLives-decrement);
	}

	void SetNLives(int nLives)
	{
		m_NLives =nLives;
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = m_Score, eNLives = m_NLives, eNEnemiesLeftBeforeVictory = m_NEnemiesLeftBeforeVictory });
	}

	// Victory Condition
	[SerializeField] private int m_NEnemiesToDestroyForVictory;
	private int m_NEnemiesLeftBeforeVictory;
	void DecrementNEnemiesLeftBeforeVictory(int decrement)
	{
		SetNEnemiesLeftBeforeVictory(m_NEnemiesLeftBeforeVictory - decrement);
	}
	void SetNEnemiesLeftBeforeVictory(int nEnemies)
	{
		m_NEnemiesLeftBeforeVictory = Mathf.Max(nEnemies,0);
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = m_Score, eNLives = m_NLives, eNEnemiesLeftBeforeVictory = m_NEnemiesLeftBeforeVictory });
	}

	//Players
	[SerializeField]
	List<PlayerController> m_Players = new List<PlayerController>();

	#region Events' subscription
	public override void SubscribeEvents()
	{
		base.SubscribeEvents();

		//PlayerController
		EventManager.Instance.AddListener<PlayerHasBeenHitEvent>(PlayerHasBeenHit);

		//MainMenuManager
		EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
		EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
		EventManager.Instance.AddListener<NextLevelButtonClickedEvent>(NextLevelButtonClicked);
		EventManager.Instance.AddListener<ResumeButtonClickedEvent>(ResumeButtonClicked);
		EventManager.Instance.AddListener<EscapeButtonClickedEvent>(EscapeButtonClicked);

		//Enemy
		EventManager.Instance.AddListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);		

		//Score Item
		EventManager.Instance.AddListener<ScoreItemEvent>(ScoreHasBeenGained);

		//Pattern
		EventManager.Instance.AddListener<PatternHasBeenInstantiatedEvent>(PatternHasBeenInstantiated);
		EventManager.Instance.AddListener<AllEnemiesOfPatternHaveBeenDestroyedEvent>(AllEnemiesOfPatternHaveBeenDestroyed);
	}

	public override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();

		//PlayerController
		EventManager.Instance.RemoveListener<PlayerHasBeenHitEvent>(PlayerHasBeenHit);

		//MainMenuManager
		EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
		EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
		EventManager.Instance.RemoveListener<NextLevelButtonClickedEvent>(NextLevelButtonClicked);
		EventManager.Instance.RemoveListener<ResumeButtonClickedEvent>(ResumeButtonClicked);
		EventManager.Instance.RemoveListener<EscapeButtonClickedEvent>(EscapeButtonClicked);

		//Enemy
		EventManager.Instance.RemoveListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);

		//Score Item
		EventManager.Instance.RemoveListener<ScoreItemEvent>(ScoreHasBeenGained);

		//Pattern
		EventManager.Instance.RemoveListener<PatternHasBeenInstantiatedEvent>(PatternHasBeenInstantiated);
		EventManager.Instance.RemoveListener<AllEnemiesOfPatternHaveBeenDestroyedEvent>(AllEnemiesOfPatternHaveBeenDestroyed);
	}
	#endregion

	#region Manager implementation
	protected override IEnumerator InitCoroutine()
	{
		Menu();
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = 0, eNLives = 0, eNEnemiesLeftBeforeVictory = 0 });
		yield break;
	}
	#endregion

	private void InitNewGame()
	{
		SetScore(0);
		SetNLives(m_NStartLives);
		SetNEnemiesLeftBeforeVictory(m_NEnemiesToDestroyForVictory);
	}

	#region Callbacks to events issued by Enemy
	private void EnemyHasBeenDestroyed(EnemyHasBeenDestroyedEvent e)
	{
		if (e.eDestroyedByPlayer)
		{
			DecrementNEnemiesLeftBeforeVictory(1);

			if (m_NEnemiesLeftBeforeVictory == 0)
			{
				Victory();
			}
		}
	}
	#endregion

	#region Callbacks to events issued by Score items
	private void PlayerHasBeenHit(PlayerHasBeenHitEvent e)
	{
		DecrementNLives(1);

		if (m_NLives == 0)
		{

			Over();
		}
	}
	#endregion

	#region Callbacks to events issued by Score items
	private void ScoreHasBeenGained(ScoreItemEvent e)
	{
		IncrementScore(e.eScore.Score);
	}
	#endregion

	#region Callbacks to events issued by Pattern
	private void PatternHasBeenInstantiated(PatternHasBeenInstantiatedEvent e)
	{ }
	private void AllEnemiesOfPatternHaveBeenDestroyed(AllEnemiesOfPatternHaveBeenDestroyedEvent e)
	{ }
	#endregion



	#region Callbacks to Events issued by MenuManager
	private void MainMenuButtonClicked(MainMenuButtonClickedEvent e)
	{
		Menu();
	}

	private void PlayButtonClicked(PlayButtonClickedEvent e)
	{
		Play();
	}

	private void NextLevelButtonClicked(NextLevelButtonClickedEvent e)
	{
		EventManager.Instance.Raise(new GoToNextLevelEvent());
	}

	private void ResumeButtonClicked(ResumeButtonClickedEvent e)
	{
		Resume();
	}

	private void EscapeButtonClicked(EscapeButtonClickedEvent e)
	{
		if (IsPlaying)
			Pause();
	}
	#endregion



	//EVENTS
	private void Menu()
	{
		SetTimeScale(0);
		m_GameState = GameState.gameMenu;
		MusicLoopsManager.Instance.PlayMusic(Constants.MENU_MUSIC);
		EventManager.Instance.Raise(new GameMenuEvent());
	}

	private void Play()
	{
		InitNewGame();
		SetTimeScale(1);
		m_GameState = GameState.gamePlay;
		MusicLoopsManager.Instance.PlayMusic(Constants.GAMEPLAY_MUSIC);
		EventManager.Instance.Raise(new GamePlayEvent());
	}

	private void Pause()
	{
		SetTimeScale(0);
		m_GameState = GameState.gamePause;
		EventManager.Instance.Raise(new GamePauseEvent());
	}

	private void Resume()
	{
		SetTimeScale(1);
		m_GameState = GameState.gamePlay;
		EventManager.Instance.Raise(new GameResumeEvent());
	}

	private void Over()
	{
		SetTimeScale(0);
		m_GameState = GameState.gameOver;
		SfxManager.Instance.PlaySfx(Constants.GAMEOVER_SFX);
		EventManager.Instance.Raise(new GameOverEvent());
	}

	private void Victory()
	{
		SetTimeScale(0);
		m_GameState = GameState.gameVictory;
		SfxManager.Instance.PlaySfx(Constants.VICTORY_SFX);
		EventManager.Instance.Raise(new GameVictoryEvent());
	}

}
