using UnityEngine;


public class autoRecycle : MonoBehaviour
{
    public float recycleTime = 30f;
    public bool destroyOndisable = true;
    private float timer;

    // Start is called before the first frame update
    void Onenable()
    {
        timer = 0f;
    }
    void OnDisable()
    {
        if ( destroyOndisable )
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recycleTime) 
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }
}
