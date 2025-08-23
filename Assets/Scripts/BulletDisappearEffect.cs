using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BulletDisappearEffect : MonoBehaviour
{
    public float duration = 0.3f;
    public float finalScale = 1.5f;

    void OnEnable()
    {
        var sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.zero;
        SetAlpha(sr, 0f);
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(finalScale, duration).SetEase(Ease.OutQuad));
        seq.Join(sr.DOFade(1f, duration).SetEase(Ease.OutQuad));
        seq.OnComplete(() => PoolManager.Instance.Recycle(gameObject)); 
    }
    void SetAlpha(SpriteRenderer g, float a)
    {
        var c = g.color;
        c.a = a;
        g.color = c;
    }
}
