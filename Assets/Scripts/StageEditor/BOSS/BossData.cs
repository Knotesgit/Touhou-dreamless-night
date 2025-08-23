using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "STG/Boss Data")]
public class BossData : ScriptableObject
{
    public GameObject bossPrefab;

    public Vector3 spawnPosition;
    public Vector3 entryTargetPosition;
    public float entryDuration = 1f;
    public GameObject spellBG;
    public List<BossActionEvent> events;
}