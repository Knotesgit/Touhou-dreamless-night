using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TreeForwardScroll : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public float baseSpeed = 1f;
    public float resetY = -5f;
    public float startY = 5f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    public float fadeInTime = 1f;
    public float startAlpha = 0.5f;

    private float scale;

    // 新增: 用于伪随机
    private float[] xSlots;
    private int currentIndex = 0;

    void Start()
    {
        // 生成伪随机分布的X坐标
        int slotCount = 8; // 你可以调多点
        xSlots = new float[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            float t = (float)i / (slotCount - 1); // 均匀分布0~1
            xSlots[i] = Mathf.Lerp(-3.195f, 0.8050001f, t);
        }
        Shuffle(xSlots); // 打乱一下顺序

        ResetTree(); // 初始就放好
    }

    void Update()
    {
        float t = Mathf.InverseLerp(startY, resetY, transform.position.y);
        scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = Vector3.one * scale;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(scale * 100);

        float adjustedSpeed = baseSpeed * scale;
        transform.Translate(Vector2.down * adjustedSpeed * Time.deltaTime);

        if (transform.position.y < resetY)
        {
            ResetTree();
        }
    }

    void ResetTree()
    {
        spriteRenderer.DOKill();

        // 从 slots 中取出一个位置再加点微扰
        float x = xSlots[currentIndex] + Random.Range(-0.2f, 0.2f);
        float y = startY + Random.Range(-0.3f, 0.3f);
        transform.position = new Vector3(x, y, transform.position.z);

        currentIndex++;
        if (currentIndex >= xSlots.Length)
        {
            currentIndex = 0;
            Shuffle(xSlots); // 每轮换新顺序
        }

        SetAlpha(spriteRenderer, startAlpha);
        spriteRenderer.DOFade(1f, fadeInTime);
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    // Fisher-Yates Shuffle
    void Shuffle(float[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(i, array.Length);
            float temp = array[rnd];
            array[rnd] = array[i];
            array[i] = temp;
        }
    }
}