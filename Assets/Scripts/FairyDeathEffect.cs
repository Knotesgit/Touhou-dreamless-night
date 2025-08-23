using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FairyDeathEffect : MonoBehaviour
{
    public SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnEnable()
    {
        sprite.color = new Color(1f, 1f, 1f, 1f);
        transform.localScale = Vector3.one * 0.1f;

        sprite.DOFade(0f, 0.25f);
        transform.DOScale(2.5f, 0.25f).SetEase(Ease.OutQuad)
            .OnComplete(() => PoolManager.Instance.Recycle(gameObject));
    }
}
