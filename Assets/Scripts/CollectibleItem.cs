using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollectibleItem : MonoBehaviour
{
    private Transform player;
    public float flySpeed = 10f;
    public float attractRange = 2f;
    public float fallSpeed = 1f;
    public bool forcedCollect = false;
    public bool collected = false;
    public CollectableType collectableType;
    public AudioClip appearSE;
    public enum CollectableType
    {
        heart,
        heartFrag,
        star,
        score
    
    }

    private void OnEnable()
    {
        collected = false;
        GeneralAudioPool.Instance.PlayOneShot(appearSE,2f);
    }

    void Update()
    {
        if (player == null && GameObject.FindWithTag("Player") != null)
            player = GameObject.FindWithTag("Player").transform;

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (forcedCollect || dist <= attractRange)
        {
            // Îü¸½
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * flySpeed * Time.deltaTime);
        }
        else 
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
        if (transform.position.y < -3.2f) 
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }

    public void ForceCollect()
    {
        forcedCollect = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }
}