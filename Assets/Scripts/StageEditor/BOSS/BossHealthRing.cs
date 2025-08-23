using UnityEngine;
using DG.Tweening;

public class BossHealthRing : MonoBehaviour
{
    public SpriteRenderer ringRenderer;
    public float maxHP = 3000f;
    private float currentHP;

    private MaterialPropertyBlock block;
    private float currentFill = 1f;
    private Tween fillTween;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    public void Initialize(float maxHealth)
    {
        maxHP = maxHealth;
        currentHP = maxHealth;
        currentFill = 0f;
        UpdateVisual(currentFill);

        // 先杀掉旧动画
        fillTween?.Kill();

        // 从 0 补间到 1
        fillTween = DOTween.To(() => currentFill, x =>
        {
            currentFill = x;
            UpdateVisual(currentFill);
        }, 1f, 1f).SetEase(Ease.OutCubic);
    }

    public void SetHP(float hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        float targetFill = Mathf.Clamp01(currentHP / maxHP);

        // 停止旧动画
        fillTween?.Kill();

        // 开启新补间动画
        fillTween = DOTween.To(() => currentFill, x =>
        {
            currentFill = x;
            UpdateVisual(currentFill);
        }, targetFill, 0.5f).SetEase(Ease.OutQuad);
    }

    private void UpdateVisual(float fill)
    {
        ringRenderer.GetPropertyBlock(block);
        block.SetFloat("_Fill", fill);
        ringRenderer.SetPropertyBlock(block);
    }

}

