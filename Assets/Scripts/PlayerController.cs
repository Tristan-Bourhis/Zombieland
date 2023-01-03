using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SDD.Events;

public class PlayerController : SimpleGameStateObserver {

	Rigidbody m_Rigidbody;

	[Header("Axes")]
	[SerializeField] private string m_VerticalAxisName;
	[SerializeField] private string m_HorizontalAxisName;
	[SerializeField] private string m_FireAxisName;

	[Header("Spawn")]
	[SerializeField] private Transform m_SpawnPoint;

	[Header("Movement")]
	[SerializeField] private float m_MaxTranslationSpeed;

	[Header("Shoot")]
	[SerializeField] private GameObject m_BulletPrefab;
	[SerializeField] private float m_ShootPeriod;
	private float m_NextShootTime;
	[SerializeField] private Transform m_BulletSpawnPoint;

	[Header("Gfx")]
	[SerializeField] private Transform m_Gfx;
	[SerializeField] private float m_GfxSwayAmplitude;
	[SerializeField] private float m_GfxSwayPulsation;
	Quaternion m_InitLocalOrientation;

	float m_GameZoneHalfHeight;
	float m_GameZoneHalfWeight;

	protected override void Awake()
	{
		base.Awake();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_InitLocalOrientation = m_Gfx.localRotation;

		Vector3 cornerWorldPos = Camera.main.ViewportToWorldPoint(new Vector3(0,0,-Camera.main.transform.position.z));
		m_GameZoneHalfHeight = Mathf.Abs(cornerWorldPos.y);
		m_GameZoneHalfWeight = Mathf.Abs(cornerWorldPos.x);
	}

	private void Update()
	{
		if (!GameManager.Instance.IsPlaying) return;

		//Fire
		if (Input.GetButton(m_FireAxisName) && m_NextShootTime<Time.time)
		{
			ShootBullet();
			m_NextShootTime = Time.time + m_ShootPeriod;
		}

		//Gfx rotation
		m_Gfx.localRotation = Quaternion.AngleAxis(Mathf.Sin(Time.time*m_GfxSwayPulsation)*m_GfxSwayAmplitude,Vector3.right)*m_InitLocalOrientation;
	}

	private void FixedUpdate()
	{
		if (!GameManager.Instance.IsPlaying)
		{
			m_Rigidbody.velocity = Vector3.zero;
			return;
		}

		float hAxis = Input.GetAxis(m_HorizontalAxisName);
		float vAxis = Input.GetAxis(m_VerticalAxisName);

		Vector3 inputVector = new Vector3(hAxis, vAxis, 0);

		Vector3 velocity =Vector3.ClampMagnitude( inputVector,1) * m_MaxTranslationSpeed;
		m_Rigidbody.velocity = velocity ;
		//Debug.Log("velocity = " + m_Rigidbody.velocity);

		float deltaYPos = transform.position.y-m_GameZoneHalfHeight;
		if(deltaYPos>0)
		{
			m_Rigidbody.MovePosition(transform.position-deltaYPos*Vector3.up);
		}
		deltaYPos = transform.position.y-(-m_GameZoneHalfHeight);
		if(deltaYPos<0)
		{
			m_Rigidbody.MovePosition(transform.position-deltaYPos*Vector3.up);
		}
		float deltaXPos =  transform.position.x-m_GameZoneHalfWeight;
		if(deltaXPos>0)
		{
			m_Rigidbody.MovePosition(transform.position-deltaXPos*Vector3.right);
		}
		deltaXPos =  transform.position.x-(-m_GameZoneHalfWeight);
		if(deltaXPos<0)
		{
			m_Rigidbody.MovePosition(transform.position-deltaXPos*Vector3.right);
		}
		
	}

	private void Reset()
	{
		m_Rigidbody.position = m_SpawnPoint.position;
		m_NextShootTime = Time.time;
	}

	void ShootBullet()
	{
		GameObject bulletGO = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.identity);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Bullet"))
		{
			EventManager.Instance.Raise(new PlayerHasBeenHitEvent() { ePlayerController = this });
		}
		if(collision.gameObject.CompareTag("Bonus"))
		{
			EventManager.Instance.Raise(new PlayerHasBonusEvent() { ePlayerController = this });
		}
	}

	//Game state events
	protected override void GamePlay(GamePlayEvent e)
	{
		Reset();
	}

}
