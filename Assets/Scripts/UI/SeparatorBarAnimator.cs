using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SeparatorBarAnimator : MonoBehaviour
{
    public Image leftLine;
    public Image rightLine;

    public float targetWidth = 300f;
    public float duration = 1f;

    void Start()
    {
        // 初始宽度为0，完全不透明
        leftLine.rectTransform.sizeDelta = new Vector2(0f, leftLine.rectTransform.sizeDelta.y);
        rightLine.rectTransform.sizeDelta = new Vector2(0f, rightLine.rectTransform.sizeDelta.y);
        SetAlpha(leftLine, 1f);
        SetAlpha(rightLine, 1f);

        // 同时展开并渐隐
        leftLine.rectTransform.DOSizeDelta(new Vector2(targetWidth, leftLine.rectTransform.sizeDelta.y), duration);
        rightLine.rectTransform.DOSizeDelta(new Vector2(targetWidth, rightLine.rectTransform.sizeDelta.y), duration);
        leftLine.DOFade(0.5f, duration);
        rightLine.DOFade(0.5f, duration);
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
