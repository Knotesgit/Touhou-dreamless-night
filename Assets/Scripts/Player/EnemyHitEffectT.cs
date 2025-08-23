using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyHitEffectT : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer sprite;
    void Start()
    {
    }

    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        sprite.color = new Color(1f, 1f, 1f, 0.7f);
        transform.DOScale(3, 0.25f);
        sprite.DOFade(0f, 0.25f).OnComplete(()=>PoolManager.Instance.Recycle(gameObject));
    }
}
