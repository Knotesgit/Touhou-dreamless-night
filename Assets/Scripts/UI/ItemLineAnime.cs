using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ItemLineAnime : MonoBehaviour
{
    public Image text;
    public Image leftLine;
    public Image rightLine;

    public float targetWidth = 120f;
    public float expandDuration = 0.4f;
    public int blinkTimes = 5;
    public float blinkDuration = 0.3f;

    void Start()
    {
        // 初始设置：线为宽度0，完全不透明
        SetLineWidth(leftLine, 0f);
        SetLineWidth(rightLine, 0f);
        SetAlpha(leftLine, 1f);
        SetAlpha(rightLine, 1f);
        SetAlpha(text, 1f);

        // 展开动画
        Sequence expandSeq = DOTween.Sequence();
        expandSeq.Append(leftLine.rectTransform.DOSizeDelta(
            new Vector2(targetWidth, leftLine.rectTransform.sizeDelta.y), expandDuration));
        expandSeq.Join(rightLine.rectTransform.DOSizeDelta(
            new Vector2(targetWidth, rightLine.rectTransform.sizeDelta.y), expandDuration));

        expandSeq.OnComplete(() =>
        {
            BlinkAndHide(text, blinkTimes, blinkDuration);
            BlinkAndHide(leftLine, blinkTimes, blinkDuration);
            BlinkAndHide(rightLine, blinkTimes, blinkDuration);
        });
    }

    void BlinkAndHide(Graphic target, int times, float duration)
    {
        SetAlpha(target, 1f);
        target.DOFade(0f, duration)
            .SetLoops(times * 2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => target.DOFade(0f,duration));
    }

    void SetLineWidth(Image line, float width)
    {
        var rt = line.rectTransform;
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }

    void SetAlpha(Graphic g, float a)
    {
        var c = g.color;
        c.a = a;
        g.color = c;
    }
}
