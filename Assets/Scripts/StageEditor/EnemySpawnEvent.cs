using UnityEngine;

[System.Serializable]
public class EnemySpawnEvent
{
    public float time; // 秒
    public GameObject enemyPrefab;
    public float hp;
    public GameObject dropItem;
    public Vector2 tragetSpeed;
    public SpawnType spawnType; // 淡入 / 飞入 / 直接出现
    public Vector2 spawnPosition; // 出现坐标
    public Vector2 moveFrom;     // 若为飞入，则从哪里飞进来
    public float moveDuration;   // 飞入/淡入时间
    public float stayDuration;
    public bool useAcceleration;
    public float accelerateDuration;
    public BulletPattern pattern;
    public enum SpawnType
    {
        Instant,    // 直接出现
        FadeIn,     // 淡入
        FlyIn       // 从 moveFrom → spawnPosition
    }
    [SerializeField, HideInInspector]
    public string groupName = "";
}
