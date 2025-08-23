using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIFadingEffect : MonoBehaviour
{
    public Image img;
    public float fadeInTime;
    public float fadeInTargetAlpha = 1f;
    public float fadeIntervalTime;
    public float fadeOutTime;
    public int loopCount = 1;
    public bool destroy = true;

    private void OnEnable()
    {
        if (img == null)
        {
            Destroy(gameObject);
            return;
        }

        Color c = img.color;
        c.a = 0f;
        img.color = c;

        Sequence sq = DOTween.Sequence();
        sq.Append(img.DOFade(fadeInTargetAlpha, fadeInTime));
        sq.AppendInterval(fadeIntervalTime);
        sq.Append(img.DOFade(0f, fadeOutTime));
        sq.SetLoops(loopCount).OnComplete(() =>
        {
            if (destroy)
            {
                Destroy(gameObject);
            }
        });
    }
}
