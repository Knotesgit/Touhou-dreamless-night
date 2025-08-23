using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "STG/Stage Data")]
public class StageData : ScriptableObject
{
    public List<EnemySpawnEvent> enemyEvents;
    public List<BackgroundEvent> backgroundEvents;
    public List<AudioEvent> audioEvents;
    public List<DisplayEvent> displayEvents;
    public List<BossEvent> bossEvents;
}
