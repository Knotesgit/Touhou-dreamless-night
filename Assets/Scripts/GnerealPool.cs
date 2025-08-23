using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralPool : MonoBehaviour
{
    public static GeneralPool Instance;

    public GameObject effectPrhabs;
    public int poolSize = 20;

    public Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(effectPrhabs);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    public void Spawn(Vector2 position)
    {
        GameObject obj = pool.Dequeue();
        obj.transform.position = position;
        obj.SetActive(true);
        pool.Enqueue(obj);
    }

    public GameObject SpawnObj() 
    {
        GameObject obj = pool.Dequeue();
        return obj;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
