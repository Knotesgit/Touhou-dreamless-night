using UnityEngine;
using DG.Tweening;

public class BackgroundManager : MonoBehaviour
{
    [Header("Renderers (两层背景)")]
    public SpriteRenderer frontRenderer;
    public SpriteRenderer backRenderer;

    [Header("Materials")]
    public Material scrollingMaterial;
    public Material staticMaterial;

    private float currentRotation = 0f;

    /// <summary>
    /// 进行带过渡的背景切换，支持滚动/静态，带淡入淡出
    /// </summary>
    public void CrossFadeToSprite(Sprite newSprite, bool useScroll, Vector2 scrollSpeed, float fadeDuration = 1f)
    {
        // 1. 交换前后 renderer
        SpriteRenderer temp = backRenderer;
        backRenderer = frontRenderer;
        frontRenderer = temp;

        // 2. 安全重置图层状态
        backRenderer.DOKill();
        frontRenderer.DOKill();
        backRenderer.color = new Color(1f, 1f, 1f, 1f); // 避免残留透明
        frontRenderer.color = new Color(1f, 1f, 1f, 0f);

        // 3. 设置前层图与材质
        frontRenderer.sprite = newSprite;

        if (useScroll && scrollingMaterial != null)
        {
            frontRenderer.material = Instantiate(scrollingMaterial);
            frontRenderer.material.SetVector("_ScrollSpeed", new Vector4(scrollSpeed.x, scrollSpeed.y, 0, 0));
        }
        else if (staticMaterial != null)
        {
            frontRenderer.material = Instantiate(staticMaterial);
        }
        else
        {
            Debug.LogWarning("BackgroundManager: No valid material set!");
            frontRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // 4. 应用当前旋转
        frontRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        backRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // 5. 淡入淡出过渡
        frontRenderer.DOFade(1f, fadeDuration).SetEase(Ease.Linear);
        backRenderer.DOFade(0f, fadeDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            backRenderer.sprite = null; // 可选：清除旧图
        });
    }

    /// <summary>
    /// 设置滚动速度（作用于前层）
    /// </summary>
    public void SetScrollSpeed(Vector2 speed)
    {
        if (frontRenderer.material.HasProperty("_ScrollSpeed"))
        {
            frontRenderer.material.SetVector("_ScrollSpeed", new Vector4(speed.x, speed.y, 0, 0));
        }
    }

    /// <summary>
    /// 立即设置旋转角度（通过 Transform）
    /// </summary>
    public void RotateTo(float angle)
    {
        currentRotation = angle;
        frontRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        backRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    /// <summary>
    /// 旋转到目标角度（插值方式）
    /// </summary>
    public void RotateToOverTime(float targetAngle, float duration)
    {
        DOTween.To(() => currentRotation, x =>
        {
            currentRotation = x;
            frontRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            backRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        }, targetAngle, duration);
    }

    /// <summary>
    /// 设置前层透明度（立即）
    /// </summary>
    public void SetAlpha(float alpha)
    {
        Color c = frontRenderer.color;
        c.a = alpha;
        frontRenderer.color = c;
    }

    /// <summary>
    /// 前层淡入淡出
    /// </summary>
    public void FadeToAlpha(float targetAlpha, float duration)
    {
        frontRenderer.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
    }
}