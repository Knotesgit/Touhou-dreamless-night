using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HitboxVisualController : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer sprite;
    public float fadeTime = 0.2f;
    public float minScale = 0.5f;
    public float maxScale = 1f;
    private Tween fadeTween;
    private Tween scaleTween;

    private bool isVisible;
    void Start()
    {
        var color = sprite.color;
        color.a = 0f;
        sprite.color = color;
        transform.localScale = Vector3.one * minScale;
        isVisible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   

    public void Show()
    {
        if (isVisible) return;
        isVisible = true;

        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = sprite.DOFade(0.7f, fadeTime).SetEase(Ease.OutSine);
        scaleTween = transform.DOScale(maxScale, fadeTime).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        if (!isVisible) return;
        isVisible = false;

        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = sprite.DOFade(0f, fadeTime).SetEase(Ease.InSine);
        scaleTween = transform.DOScale(minScale, fadeTime).SetEase(Ease.InQuad);
    }
}
