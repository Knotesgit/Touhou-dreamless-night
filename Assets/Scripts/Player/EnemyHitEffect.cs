using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;

public class EnemyHitEffect : MonoBehaviour
{
    public SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        sprite.color = new Color(1f, 1f, 1f, 0.7f);
        transform.localScale = Vector3.one * 0.7f;
        Vector3 target = transform.position + new Vector3(0f, 0.5f,0);
        transform.DOMove(target, 0.15f).SetEase(Ease.OutSine);
        sprite.DOFade(0f, 0.15f)
            .OnComplete(() => PoolManager.Instance.Recycle(gameObject));
    }
}
