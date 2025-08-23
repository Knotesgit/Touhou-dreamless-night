using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public int defaultPoolSize = 100;  // ÿ����չ�Ĵ�С
    public float expandFactor = 0.5f;  // ��չ����
    public int minPoolSize = 10;      // ������С���������������������ʱ����չ
    public float expandCooldown = 1f; // ��չ��ȴʱ�䣨��ֹ��δ�����

    private Dictionary<GameObject, Queue<GameObject>> poolDict = new();
    private Dictionary<GameObject, float> lastExpandedTime = new();  // ��¼ÿ�� prefab ���һ����չ��ʱ��

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
        // �������û�е�ǰ prefab����ʼ��һ���µĳ�
        if (!poolDict.ContainsKey(prefab))
        {
            poolDict[prefab] = new Queue<GameObject>();
            ExpandPool(prefab, defaultPoolSize);  // ��ʼʱ���� defaultPoolSize ��չ
        }

        // ��ȡ����
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

            // ���ص������Ƿ�С����ֵ����������չ��
            if (poolDict[prefab].Count < minPoolSize)
            {
                // ��ֹ����ȴ�ڼ�����չ
                if (!lastExpandedTime.ContainsKey(prefab) || Time.time - lastExpandedTime[prefab] > expandCooldown)
                {
                    ExpandPool(prefab, Mathf.CeilToInt(defaultPoolSize * expandFactor));  // ʹ����չ������չ��
                    lastExpandedTime[prefab] = Time.time;  // ���������չʱ��
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
