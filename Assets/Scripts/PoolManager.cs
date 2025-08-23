using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public int defaultPoolSize = 100;  // 每次扩展的大小
    public float expandFactor = 0.5f;  // 扩展因子
    public int minPoolSize = 10;      // 池中最小对象数量，低于这个数量时才扩展
    public float expandCooldown = 1f; // 扩展冷却时间（防止多次触发）

    private Dictionary<GameObject, Queue<GameObject>> poolDict = new();
    private Dictionary<GameObject, float> lastExpandedTime = new();  // 记录每个 prefab 最后一次扩展的时间

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject Get(GameObject prefab)
    {
        return Get(prefab, Vector2.zero);
    }

    public GameObject Get(GameObject prefab, Vector2 position)
    {
        if (prefab == null) 
        {
            return null;
        }
        // 如果池中没有当前 prefab，初始化一个新的池
        if (!poolDict.ContainsKey(prefab))
        {
            poolDict[prefab] = new Queue<GameObject>();
            ExpandPool(prefab, defaultPoolSize);  // 初始时根据 defaultPoolSize 扩展
        }

        // 获取对象
        GameObject objToUse;

        if (poolDict[prefab].Count > 0)
        {
            objToUse = poolDict[prefab].Dequeue();
        }
        else
        {
            objToUse = Instantiate(prefab);
            var pooled = objToUse.AddComponent<PooledObject>();
            pooled.originalPrehab = prefab;

            // 检查池的数量是否小于阈值，若是则扩展池
            if (poolDict[prefab].Count < minPoolSize)
            {
                // 防止在冷却期间多次扩展
                if (!lastExpandedTime.ContainsKey(prefab) || Time.time - lastExpandedTime[prefab] > expandCooldown)
                {
                    ExpandPool(prefab, Mathf.CeilToInt(defaultPoolSize * expandFactor));  // 使用扩展因子扩展池
                    lastExpandedTime[prefab] = Time.time;  // 更新最后扩展时间
                    Debug.LogWarning($"[PoolManager] Auto-expanded pool for {prefab.name}");
                }
            }
        }

        objToUse.transform.position = position;
        objToUse.SetActive(true);

        return objToUse;
    }

    private void ExpandPool(GameObject prefab, int expandBy)
    {
        for (int i = 0; i < expandBy; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);

            var pooled = obj.AddComponent<PooledObject>();
            pooled.originalPrehab = prefab;

            poolDict[prefab].Enqueue(obj);
        }
    }

    public void Recycle(GameObject bullet)
    {
        bullet.SetActive(false);

        PooledObject pooled = bullet.GetComponent<PooledObject>();
        if (pooled != null && pooled.originalPrehab != null && poolDict.ContainsKey(pooled.originalPrehab))
        {
            poolDict[pooled.originalPrehab].Enqueue(bullet);
        }
        else
        {
            Debug.LogWarning($"[PoolManager] Recycle failed: prefab not tracked, destroying {bullet.name}");
            Destroy(bullet);
        }
    }
}
