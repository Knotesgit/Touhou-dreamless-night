using DG.Tweening;
using UnityEngine;

public class FadingEffect : MonoBehaviour
{
    public SpriteRenderer sr;
    public float fadeInTime;
    public float fadInTargetAlpha=1f;
    public float fadeIntervalTime;
    public float fadeOutTime;
    public int loopCount = 1;
    public bool destroy = true;

    private void OnEnable()
    {
        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        sr.color = new Color(1f, 1f, 1f, 0f);
        Sequence sq = DOTween.Sequence();
        sq.Append(sr.DOFade(fadInTargetAlpha, fadeInTime));
        sq.AppendInterval(fadeIntervalTime);
        sq.Append(sr.DOFade(0f, fadeOutTime));
        sq.SetLoops(loopCount).OnComplete(() =>
        {
            if (destroy)
            {
                Destroy(gameObject);
            }
        });
    }
}
