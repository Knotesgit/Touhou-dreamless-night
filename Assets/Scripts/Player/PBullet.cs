using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBullet : MonoBehaviour
{
    
    public GameObject hitEffect;
    // Start is called before the first frame update
    public float speed = 20f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        if (transform.position.y > 3 || transform.position.y < -3
            || transform.position.x > 2 || transform.position.x < -4)
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            IHittable enemy = collision.gameObject.GetComponent<IHittable>();

            if (enemy != null) // Ensure the object has PlayerController
            {
                enemy.OnHit(0.3f);
                PoolManager.Instance.Get(hitEffect,transform.position);
                PoolManager.Instance.Recycle(gameObject);
            }
        }
        
    }
}

