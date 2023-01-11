using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public abstract class Enemy : SimpleGameStateObserver,IScore {

	protected Rigidbody m_Rigidbody;
	protected Transform m_Transform;

	[Header("Movement")]
	[SerializeField] private float m_MaxTranslationSpeed;
	[SerializeField] private float m_MinTranslationSpeed;
	[SerializeField] private AnimationCurve m_TranslationSpeedProbaCurve;
	protected float m_TranslationSpeed;
	public float TranslationSpeed { get { return m_TranslationSpeed; } }

	protected abstract Vector3 MoveVect { get; }

	[Header("Score")]
	[SerializeField] int m_Score;
	public int Score { get { return m_Score; } }

	[Header("Shoot")]
	[SerializeField] private GameObject m_BulletPrefab;
	[SerializeField] private float m_ShootPeriod;
	private float m_NextShootTime = 0;
	[SerializeField] private Transform m_BulletSpawnPoint;

	protected bool m_Destroyed = false;
	private int boss_HP = 15;

	protected override void Awake()
	{
		base.Awake();

		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = GetComponent<Transform>();

		m_TranslationSpeed = Mathf.Lerp(m_MinTranslationSpeed, m_MaxTranslationSpeed, m_TranslationSpeedProbaCurve.Evaluate(Random.value));
	}

	public virtual void Update()
	{
		if (Camera.main.WorldToViewportPoint(m_Transform.position).x < -.1f)
		{
			EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() { eEnemy = this, eDestroyedByPlayer = false });
			m_Destroyed = true;
			Destroy(gameObject);
		}

		if (m_NextShootTime<Time.time)
		{
			ShootBullet();
			m_NextShootTime = Time.time + m_ShootPeriod;
		}
	}

	public virtual void FixedUpdate()
	{
		if (!GameManager.Instance.IsPlaying) return;
		m_Rigidbody.MovePosition(m_Rigidbody.position + MoveVect);
	}

	void ShootBullet()
	{
		GameObject bulletGO = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(180,Vector3.forward));
		if(gameObject.CompareTag("Boss")==true)
		{
			GameObject bulletGO2 = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(165,Vector3.forward));
			GameObject bulletGO3 = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(195,Vector3.forward));
		}
		if(boss_HP <= 10) 
		{
			GameObject bulletGO4 = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(150,Vector3.forward));
			GameObject bulletGO5 = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(210,Vector3.forward));
		}
		if(boss_HP <= 5) 
		{
			GameObject bulletGO6 = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(170,Vector3.forward));
			GameObject bulletGO7 = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.AngleAxis(190,Vector3.forward));
		}
		
	}

	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log( name+" Collision with " + collision.gameObject.name);
		if(collision.gameObject.CompareTag("Bullet")
			|| collision.gameObject.CompareTag("Player"))
		{
			if(gameObject.CompareTag("Boss")==false || boss_HP == 0)
			{
				EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
				EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() { eEnemy = this,eDestroyedByPlayer = true });						
				m_Destroyed = true;
				Destroy(gameObject);
			}else 
			{
				boss_HP -= 1;
				Debug.Log(boss_HP);
			}
			
		}
	}
}
