using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TimedBulletPattern
{
    public BulletPatternData pattern;
    public float timeOffset;
    public float repeatInterval;
    public int repeatCount = -1;
}
