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
    public float offscreenOffset = 5f; // �м�λ���������ƶ������絥λ����

    void OnEnable()
    {
        tmp.DOKill();
        transform.DOKill();
        SetSorting(tmp, "EBullet", 100);
        // �м�λ��
        Vector3 midPos = transform.position;

        // ���Ҳ࿪ʼ
        transform.position = midPos + new Vector3(offscreenOffset, 0, 0);
        tmp.alpha = 0f;

        var seq = DOTween.Sequence();

        // �������м� & ����
        seq.Append(transform.DOMoveX(midPos.x, moveInDuration).SetEase(Ease.OutCubic));
        seq.Join(tmp.DOFade(1f,fadeDuration));

        // ͣ��
        seq.AppendInterval(stayDuration);

        // �˳������ & ����
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
