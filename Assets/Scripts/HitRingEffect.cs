using UnityEngine;
using DG.Tweening;

public class HitRingEffect : MonoBehaviour
{
    public SpriteRenderer[] rings; // ����5�� SpriteRenderer

    public float duration = 0.5f;
    public float finalScale = 3f;

    void OnEnable()
    {
        for (int i = 0; i < rings.Length; i++)
        {
            rings[i].DOKill();
            rings[i].transform.localScale = Vector3.zero;
            rings[i].color = new Color(1f, 1f, 1f, 1f);

            float delay = i * 0.03f;

            rings[i].transform.DOScale(finalScale, duration)
                .SetEase(Ease.OutQuad);

            rings[i].DOFade(0f, duration)
                .SetEase(Ease.OutQuad);
        }

        // �Զ�����
        Invoke(nameof(Finish), duration + 0.2f);
    }

    void Finish()
    {
        Destroy(gameObject);
    }
}