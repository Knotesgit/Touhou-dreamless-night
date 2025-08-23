using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBulletT : MonoBehaviour
{
    public float speed = 7f;
    public float rotateLerpSpeed = 6f; // 新增，类似“追踪强度”
    public float searchRadius = 10f;
    public float lockDistance = 0.5f; // 新增：进入此距离后锁定目标
    public GameObject hitEffect;
    private Transform target;
    private bool lockedOn = false;

    void OnEnable()
    {
        transform.up = Vector3.up;
        target = null;
        lockedOn = false;
    }

    void Update()
    {
        if (!lockedOn)
        {
            target = FindClosestTarget();
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance <= lockDistance)
                {
                    lockedOn = true; // 锁定目标
                    target = null;
                }
            }
        }

        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
            transform.up = Vector2.Lerp(transform.up, direction.normalized, rotateLerpSpeed * Time.deltaTime);
        }

        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);

        if (transform.position.y >= 3 || transform.position.y <= -3
            || transform.position.x >= 1.6 || transform.position.x <= -4)
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }

    Transform FindClosestTarget()
    {
        List<Enemy> enemies = Enemy.ActiveEnemies;
        Transform closest = null;
        float minDist = searchRadius;

        foreach (Enemy enemy in enemies)
        {
            Vector2 enemyPos = enemy.transform.position;
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;
            
            if (enemyPos.x < -4f || enemyPos.x > 1.6f || enemyPos.y < -3f || enemyPos.y > 3f)
                continue;
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }

        var boss = BossController.Instance;
        if (boss != null && boss.gameObject.activeInHierarchy)
        {
            float bossDist = Vector2.Distance(transform.position, boss.transform.position);
            Vector2 bossPos = boss.transform.position;
            if (bossPos.x >= -4f && bossPos.x <= 1.6f && bossPos.y >= -3f && bossPos.y <= 3f) 
            {
                if (bossDist < minDist)
                {
                    closest = boss.transform;
                }
            }

        }
        return closest;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        IHittable enemy = collision.GetComponent<IHittable>();
        if (enemy != null)
        {
            enemy.OnHit(0.25f);
            PoolManager.Instance.Get(hitEffect, transform.position);
            PoolManager.Instance.Recycle(gameObject);
        }

    }
}