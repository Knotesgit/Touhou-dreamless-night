using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatternSpawnInfo
{
    public GameObject patternPrefab;
    public bool attachToBoss = true;
    public Vector3 localOffset = Vector3.zero;

    public float delay = 0f;
    public bool clearBullet = false;
}