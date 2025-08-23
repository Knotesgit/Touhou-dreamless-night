using UnityEngine;
using DG.Tweening;
using TMPro;

public class BGMTitleWorldTMP : MonoBehaviour
{
    public TextMeshPro tmp;

    [Header("Movement")]
    public float moveInDuration = 0.5f;
    public float stayDuration = 2f;
    public float moveOutDuration = 1f;
    public float fadeDuration = 0.5f;
    public float offscreenOffset = 5f; // 中间位置向左右移动的世界单位距离

    void OnEnable()
    {
        tmp.DOKill();
        transform.DOKill();
        SetSorting(tmp, "EBullet", 100);
        // 中间位置
        Vector3 midPos = transform.position;

        // 从右侧开始
        transform.position = midPos + new Vector3(offscreenOffset, 0, 0);
        tmp.alpha = 0f;

        var seq = DOTween.Sequence();

        // 进场到中间 & 淡入
        seq.Append(transform.DOMoveX(midPos.x, moveInDuration).SetEase(Ease.OutCubic));
        seq.Join(tmp.DOFade(1f,fadeDuration));

        // 停留
        seq.AppendInterval(stayDuration);

        // 退场到左侧 & 淡出
        seq.Append(transform.DOMoveX(midPos.x - offscreenOffset, moveOutDuration).SetEase(Ease.InCubic));
        seq.Join(tmp.DOFade(0f,fadeDuration));

        seq.OnComplete(() => Destroy(gameObject));
    }
    void SetSorting(TextMeshPro tmp, string layer, int order)
    {
        if (tmp == null) return;
        var renderer = tmp.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = order;
        }
    }
}
