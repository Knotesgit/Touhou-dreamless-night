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
    /// ��ʼ�����ǣ��������Ų�
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
    /// ����һ�ŷ������Ƴ����Ҳ������
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

