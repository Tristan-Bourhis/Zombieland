using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusManager : Manager<BonusManager>
{
    
	[Header("Bonus")]
	[SerializeField] GameObject m_PatternsPrefabs;
    [SerializeField] private float m_BonusPeriod;
	private float m_NextBonusTime = 0;
    private Vector3 spawnPosition;

    #region Manager Implementation
	protected override IEnumerator InitCoroutine()
	{
		yield break;
	}
    #endregion

    void Start()
    {
        m_NextBonusTime = Time.time + m_BonusPeriod;     
    }

    // Update is called once per frame
    void Update()
    {
        if (m_NextBonusTime<Time.time)
		{
            spawnPosition = new Vector3(Random.Range(-8.0f, 8.0f), 15.0f,0.0f);
			Instantiate(m_PatternsPrefabs, spawnPosition, Quaternion.AngleAxis(270,Vector3.forward));
			m_NextBonusTime = Time.time + m_BonusPeriod;
		}
        
    }
}
