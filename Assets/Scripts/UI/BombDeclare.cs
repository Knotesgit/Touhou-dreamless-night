using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BombDeclare : MonoBehaviour
{
    public float fadeInDuration = 0.3f;
    public float moveDistance = 200f;
    public float moveDuration = 0.7f;
    public float fadeOutDelay = 0.5f;
    public float fadeOutDuration = 0.5f;

    private RectTransform rt;
    private CanvasGroup cg;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();

        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        cg.alpha = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos - new Vector2(0, moveDistance);

        cg.DOFade(1f, fadeInDuration);
        rt.DOAnchorPos(endPos, moveDuration).SetEase(Ease.InOutSine);
        cg.DOFade(0f, fadeOutDuration).SetDelay(fadeOutDelay);

        // ×Ô¶¯Ïú»Ù
        Destroy(gameObject, Mathf.Max(moveDuration, fadeOutDelay + fadeOutDuration) + 0.2f);
    }
}
