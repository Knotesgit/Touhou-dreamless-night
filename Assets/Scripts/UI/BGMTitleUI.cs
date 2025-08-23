using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class BGMTitleUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    public float moveInDuration = 0.5f;
    public float stayDuration = 2f;
    public float moveOutDuration = 1f;
    public float fadeDuration = 0.5f;

    private RectTransform rt;
    private CanvasGroup cg;



    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        StartCoroutine(DelayedPositionSetup());
        
    }

    IEnumerator DelayedPositionSetup()
    {
        // �ȴ�һ֡
        yield return null;  // ��Э����ͣ��ֱ����һ֡
        float width = ((RectTransform)transform.parent).rect.width;
        rt.anchoredPosition = new Vector2(width / 4 + 1f, 0); // ���Ҳ����濪ʼ
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, fadeDuration));
        seq.Join(rt.DOAnchorPosX(0, moveInDuration).SetEase(Ease.OutCubic));
        seq.AppendInterval(stayDuration);
        seq.Append(rt.DOAnchorPosX(-width / 4 - 1f, moveOutDuration).SetEase(Ease.InCubic));
        seq.Join(cg.DOFade(0f, fadeDuration));
        seq.OnComplete(() => Destroy(gameObject));
    }
}