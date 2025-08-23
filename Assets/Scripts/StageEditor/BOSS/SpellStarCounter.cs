using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SpellStarCounter : MonoBehaviour
{
    public static SpellStarCounter Instance;
    public GameObject starPrefab;
    public float starSpacing = 0.3f;
    private int maxSpellCount = 3;

    private List<GameObject> stars = new();

    private void Awake()
    {
        Instance = this;       
    }
    /// <summary>
    /// 初始化星星，从左到右排布
    /// </summary>
    public void Initialize(int spellCount)
    {
        ClearStars();
        maxSpellCount = spellCount;

        for (int i = 0; i < maxSpellCount; i++)
        {
            Vector2 pos = new Vector2(transform.position.x + i * starSpacing, transform.position.y);
            GameObject star = Instantiate(starPrefab, pos, Quaternion.identity, transform);
            stars.Add(star);
        }
    }

    /// <summary>
    /// 击破一张符卡，移除最右侧的星星
    /// </summary>
    public void UseOneSpell()
    {
        if (stars.Count == 0) return;

        int rightmost = stars.Count - 1;
        Destroy(stars[rightmost]);
        stars.RemoveAt(rightmost);
    }

    public void ClearStars()
    {
        foreach (var star in stars)
        {
            if (star != null) Destroy(star);
        }
        stars.Clear();
    }
}

