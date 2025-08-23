using UnityEngine;
using DG.Tweening;

public class FogController : MonoBehaviour
{
    public System.Action OnRecycle;
    private SpriteRenderer sr;
    private bool isBlowing = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        isBlowing = false;

        // 随机方向
        bool fromLeft = Random.value > 0.5f;
        float startX = fromLeft ? -4f : 2f;
        float endX = fromLeft ? 2f : -4f;
        float y = Random.Range(1.5f, 2f);
        float duration = Random.Range(8f, 15f);
        //float alpha = Random.Range(0.025f, 0.07f);
        float alpha = Random.Range(0.05f, 0.1f);
        float scale = Random.Range(2f, 2.5f);

        transform.position = new Vector3(startX, y, 0f);
        transform.localScale = Vector3.one * scale;
        sr.color = new Color(1, 1, 1, 0);

        // 淡入
        sr.DOFade(alpha, 2f).SetUpdate(true);

        // 正常飘动
        transform.DOMoveX(endX, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (!isBlowing)
                {
                    FadeOutAndRecycle();
                }
            });
    }

    void Recycle()
    {
        OnRecycle?.Invoke();
        PoolManager.Instance.Recycle(gameObject);
    }

    void FadeOutAndRecycle()
    {
        sr.DOFade(0f, 1.5f).SetUpdate(true).OnComplete(Recycle);
    }

    public void BlowAway(Vector2 direction, float speed)
    {
        isBlowing = true;

        transform.DOKill();
        sr.DOKill();

        Vector3 targetPos = transform.position + (Vector3)(direction.normalized * 20f);
        transform.DOMove(targetPos, 3f).SetEase(Ease.OutQuad).SetUpdate(true);
        sr.DOFade(0f, 1.5f).SetUpdate(true).OnComplete(Recycle);
    }

    void OnDisable()
    {
        transform.DOKill();
        sr.DOKill();
    }
}