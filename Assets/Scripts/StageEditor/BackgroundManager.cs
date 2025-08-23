using UnityEngine;
using DG.Tweening;

public class BackgroundManager : MonoBehaviour
{
    [Header("Renderers (���㱳��)")]
    public SpriteRenderer frontRenderer;
    public SpriteRenderer backRenderer;

    [Header("Materials")]
    public Material scrollingMaterial;
    public Material staticMaterial;

    private float currentRotation = 0f;

    /// <summary>
    /// ���д����ɵı����л���֧�ֹ���/��̬�������뵭��
    /// </summary>
    public void CrossFadeToSprite(Sprite newSprite, bool useScroll, Vector2 scrollSpeed, float fadeDuration = 1f)
    {
        // 1. ����ǰ�� renderer
        SpriteRenderer temp = backRenderer;
        backRenderer = frontRenderer;
        frontRenderer = temp;

        // 2. ��ȫ����ͼ��״̬
        backRenderer.DOKill();
        frontRenderer.DOKill();
        backRenderer.color = new Color(1f, 1f, 1f, 1f); // �������͸��
        frontRenderer.color = new Color(1f, 1f, 1f, 0f);

        // 3. ����ǰ��ͼ�����
        frontRenderer.sprite = newSprite;

        if (useScroll && scrollingMaterial != null)
        {
            frontRenderer.material = Instantiate(scrollingMaterial);
            frontRenderer.material.SetVector("_ScrollSpeed", new Vector4(scrollSpeed.x, scrollSpeed.y, 0, 0));
        }
        else if (staticMaterial != null)
        {
            frontRenderer.material = Instantiate(staticMaterial);
        }
        else
        {
            Debug.LogWarning("BackgroundManager: No valid material set!");
            frontRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // 4. Ӧ�õ�ǰ��ת
        frontRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        backRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // 5. ���뵭������
        frontRenderer.DOFade(1f, fadeDuration).SetEase(Ease.Linear);
        backRenderer.DOFade(0f, fadeDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            backRenderer.sprite = null; // ��ѡ�������ͼ
        });
    }

    /// <summary>
    /// ���ù����ٶȣ�������ǰ�㣩
    /// </summary>
    public void SetScrollSpeed(Vector2 speed)
    {
        if (frontRenderer.material.HasProperty("_ScrollSpeed"))
        {
            frontRenderer.material.SetVector("_ScrollSpeed", new Vector4(speed.x, speed.y, 0, 0));
        }
    }

    /// <summary>
    /// ����������ת�Ƕȣ�ͨ�� Transform��
    /// </summary>
    public void RotateTo(float angle)
    {
        currentRotation = angle;
        frontRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        backRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    /// <summary>
    /// ��ת��Ŀ��Ƕȣ���ֵ��ʽ��
    /// </summary>
    public void RotateToOverTime(float targetAngle, float duration)
    {
        DOTween.To(() => currentRotation, x =>
        {
            currentRotation = x;
            frontRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            backRenderer.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        }, targetAngle, duration);
    }

    /// <summary>
    /// ����ǰ��͸���ȣ�������
    /// </summary>
    public void SetAlpha(float alpha)
    {
        Color c = frontRenderer.color;
        c.a = alpha;
        frontRenderer.color = c;
    }

    /// <summary>
    /// ǰ�㵭�뵭��
    /// </summary>
    public void FadeToAlpha(float targetAlpha, float duration)
    {
        frontRenderer.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
    }
}