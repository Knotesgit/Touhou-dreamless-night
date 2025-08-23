using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FloorScrollWithFade : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public float speed = 2f;
    public float resetY = -5f;
    public float startY = 5f;

    public float minAlpha = 0.3f; // Զ����͸��
    public float maxAlpha = 1f;    // �����͸��

    void Start()
    {
        UpdateAlpha();
    }

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        if (transform.position.y < resetY)
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
        }

        UpdateAlpha();
    }

    void UpdateAlpha()
    {
        // Y Խ��Խ��͸��
        float t = Mathf.InverseLerp(startY, resetY, transform.position.y);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        SetAlpha(spriteRenderer, alpha);
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
