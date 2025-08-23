using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class MSDirectionChanger : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip masterSparkSE;
    public GameObject whiteTransition;
    private GameObject player;
    private float angle;
    private float rad;
    private bool changed =false;
    private bool playedEffect =true;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector2 toPlayer = (player.transform.position - transform.position).normalized;
            angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            rad = angle * Mathf.Deg2Rad;
            Vector2 localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            localDir = -localDir;
            transform.right= localDir;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOutOfScreen(transform.parent.position))
        {
            playedEffect = false;

        }
        if (!IsOutOfScreen(transform.parent.position)) 
        {
            changed = false;
        }
        if (IsOutOfScreen(transform.parent.position)&&!changed)
        {
            ChangeDirection();
        }
        if (!IsOutOfScreen(transform.parent.position) && !playedEffect) 
        {
            playEffects();
        }
    }

    private bool IsOutOfScreen(Vector3 worldPos)
    {
        return (transform.position.y >= 5 || transform.position.y <= -5
        || transform.position.x >= 4 || transform.position.x <= -6);
    }
    private void ChangeDirection() 
    {
        changed = true;
        if (player != null)
        {
            Vector2 toPlayer = (player.transform.position - transform.position).normalized;
            angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            rad = angle * Mathf.Deg2Rad;
            Vector2 localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            localDir = -localDir;
            transform.right = localDir;
        }
    }
    private void playEffects()
    {
        playedEffect = true;
        CameraShaker.ShakeOnce(0.05f, 3);
        GeneralAudioPool.Instance.PlayOneShot(masterSparkSE, 1f);
        GameObject.Instantiate(whiteTransition);
    }
}
