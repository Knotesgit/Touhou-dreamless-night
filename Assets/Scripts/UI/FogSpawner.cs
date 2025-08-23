using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FogSpawner : MonoBehaviour
{
    public GameObject fogPrefab;


    private Coroutine spawnCoroutine;
    private List<GameObject> activeFogs = new List<GameObject>();

    public void StartSpawning()
    {
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            var fog = PoolManager.Instance.Get(fogPrefab, Vector2.zero);
            RegisterFog(fog);
            yield return new WaitForSeconds(Random.Range(4f, 6f));
        }
    }
    void RegisterFog(GameObject fog)
    {
        if (!activeFogs.Contains(fog))
            activeFogs.Add(fog);

        FogController ctrl = fog.GetComponent<FogController>();
        if (ctrl != null)
        {
            ctrl.OnRecycle = () =>
            {
                activeFogs.Remove(fog);
            };
        }
    }

    /// <summary>
    /// �����ڳ�����ĳ��������
    /// </summary>
    public void BlowAwayAllFog(Vector2 direction, float speed)
    {
        foreach (var fog in activeFogs)
        {
            if (fog != null && fog.activeInHierarchy)
            {
                var particle = fog.GetComponent<FogController>();
                if (particle != null)
                {
                    particle.BlowAway(direction, speed);
                }
            }
        }

        // ����б��ú��������ظ�������Щ��
        //activeFogs.Clear();
    }
}