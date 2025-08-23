using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WarningEffect : MonoBehaviour
{
    public SpriteRenderer sr;
    public float scale;
    public float fadeInTime;
    public float fadeOutTime;
    public int loopCount=1;
    public AudioClip clip;
    void Start()
    {
        sr.color = new Color(1, 1, 1, 0);
        transform.localScale = new Vector3(scale,scale,1);
        Sequence sq = DOTween.Sequence();
        sq.Append(sr.DOFade(1f, fadeInTime));
        sq.AppendCallback(() => GeneralAudioPool.Instance.PlayOneShot(clip, 1f));
        sq.Append(transform.DOScale(new Vector3(0, 0, 1), fadeOutTime))
            .Join(sr.DOFade(0f,fadeOutTime)).SetLoops(loopCount);
    }
}
