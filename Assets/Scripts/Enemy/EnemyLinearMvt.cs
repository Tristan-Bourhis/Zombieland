using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLinearMvt : Enemy {

	protected override Vector3 MoveVect
	{
		get
		{
			return m_Transform.right * m_TranslationSpeed * Time.fixedDeltaTime;
		}
	}
}
