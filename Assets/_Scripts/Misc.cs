using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldConsts {
	public Vector3 sizeInit, sizeEnd;
	public Vector2 timeShrink;
    public float floatAcc = 0.02f, floatFriction = 0.9f, shootTime=0.1f;
	public float ejectSpeed,ejectRatio;
	public float playerDeadForce;
	public Color cellSmallest, cellNear, cellDanger, cellDeadly;
	public Vector2 cellDangerThreshold;
}

[System.Serializable]
public class CellSpawnGroup {
	public float massFrom,massTo;
	public int count;
	public float maxSpeed;
}

[System.Serializable]
public class PlayerSpawnParams {
	public float radius;
	public Vector3 pos;
	public float safeDistance;
}
