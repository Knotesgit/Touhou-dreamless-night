using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextMoving : MonoBehaviour
{
    public float distance;      // ����ƶ�����
    public float movingSpeed;   // ��λ/���ٶȣ����� > 0��
    private TextMeshPro text;

    void OnEnable()
    {
        text = GetComponent<TextMeshPro>();
        SetSorting(text, "Background", 910);

        // ��ֹδ�趨��Ƿ��ٶ�
        if (movingSpeed <= 0f) return;

        float duration = Mathf.Abs(distance) / movingSpeed;
        float targetX = transform.localPosition.x + distance;
        


        // ��ֹ NaN �ݻ��������� DOTween
        if (float.IsNaN(targetX) || float.IsNaN(duration)) return;
        Sequence sq = DOTween.Sequence();
        text.color = new Color(1, 1, 1, 0f);
        transform.DOLocalMoveX(targetX, duration).SetEase(Ease.Linear);
        sq.Append(text.DOFade(0.5f, 1.5f));
        sq.Append(text.DOFade(0f, 1.5f));

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




