using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPattern {

	void StartPattern();
	float NextWaitDurationBeforeSpawn { get; }
	Enemy SpawnEnemy();
	bool IsPatternOver { get; }
}
