using UnityEngine;

[System.Serializable]
public class EnemySpawnEvent
{
    public float time; // ��
    public GameObject enemyPrefab;
    public float hp;
    public GameObject dropItem;
    public Vector2 tragetSpeed;
    public SpawnType spawnType; // ���� / ���� / ֱ�ӳ���
    public Vector2 spawnPosition; // ��������
    public Vector2 moveFrom;     // ��Ϊ���룬�������ɽ���
    public float moveDuration;   // ����/����ʱ��
    public float stayDuration;
    public bool useAcceleration;
    public float accelerateDuration;
    public BulletPattern pattern;
    public enum SpawnType
    {
        Instant,    // ֱ�ӳ���
        FadeIn,     // ����
        FlyIn       // �� moveFrom �� spawnPosition
    }
    [SerializeField, HideInInspector]
    public string groupName = "";
}
