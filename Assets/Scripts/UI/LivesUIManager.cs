using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LivesUIManager : MonoBehaviour
{
    [Header("UI Resources")]
    public Sprite fullHeart;
    public Sprite[] fragmentHearts; // 0/3, 1/3, 2/3
    public Sprite[] fragmentNumber; // 0,1,2,3,"/"
    public GameObject heartImagePrefab;
    public Transform livesPanel;
    public Transform fragmentNum;
    public Image extendImg;

    [Header("Settings")]
    public int maxLives = 7;
    public AudioClip extend;
    private List<Image> heartImages = new List<Image>();
    private List<Image> fragmentImages = new List<Image>();

    // 当前状态
    public int lifeCount = 0;
    public int fragmentCount = 0;

    void Start()
    {
        SetAlpha(extendImg, 0f);
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartImagePrefab, livesPanel);
            heart.transform.localScale = Vector3.one;
            Image img = heart.GetComponent<Image>();
            heartImages.Add(img);
        }
        for (int i = 0; i < 3; i++) 
        {
            GameObject fragment = Instantiate(heartImagePrefab,fragmentNum);
            fragment.transform.localScale = Vector3.one;
            Image num = fragment.GetComponent<Image>();
            fragmentImages.Add(num);
        }
        UpdateLivesUI();
    }

    /// <summary>
    /// 增加一个残机碎片，3个则进1命
    /// </summary>
    public void AddFragment()
    {
        fragmentCount++;
        if (fragmentCount >= 3)
        {
            fragmentCount = fragmentCount % 3;
            AddLife();
        }
        else
        {
            UpdateLivesUI();
        }
    }

    /// <summary>
    /// 增加一个完整残机
    /// </summary>
    public void AddLife()
    {
        if (lifeCount < maxLives)
        {
            lifeCount++;
            GeneralAudioPool.Instance.PlayOneShot(extend);
            Extended();
        }
        UpdateLivesUI();
    }
    /// <summary>
    /// 减少一个完整残机
    /// </summary>
    public void ReduceLife()
    {
        lifeCount--;
        UpdateLivesUI();
    }

    /// <summary>
    /// 更新 UI 显示
    /// </summary>
    private void UpdateLivesUI()
    {
        for (int i = 0; i < maxLives; i++)
        {
            if (i < lifeCount)
            {
                heartImages[i].sprite = fullHeart;
                heartImages[i].enabled = true;
            }
            else if (i == lifeCount && lifeCount < maxLives)
            {
                heartImages[i].sprite = fragmentHearts[fragmentCount];
                heartImages[i].enabled = true;
            }
            else if (i < maxLives)
            {
                heartImages[i].sprite = fragmentHearts[0];
                heartImages[i].enabled = true;
            }
        }
        int numIndex = Mathf.Clamp(fragmentCount, 0, 2);
        fragmentImages[0].sprite = fragmentNumber[numIndex];     // 0, 1, 2
        fragmentImages[1].sprite = fragmentNumber[4];            // "/"
        fragmentImages[2].sprite = fragmentNumber[3];            // "3"

        fragmentImages[0].enabled = true;
        fragmentImages[1].enabled = true;
        fragmentImages[2].enabled = true;

        fragmentImages[0].rectTransform.localScale = Vector3.one * 0.6f;
        fragmentImages[1].rectTransform.localScale = Vector3.one * 0.6f;
        fragmentImages[2].rectTransform.localScale = Vector3.one * 0.6f;
    }

    // 可选暴露：用于测试或重置
    public void SetState(int life, int fragment)
    {
        lifeCount = Mathf.Clamp(life, 0, maxLives);
        fragmentCount = Mathf.Clamp(fragment, 0, 2);
        UpdateLivesUI();
    }
    void SetAlpha(Graphic g, float a)
    {
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
    void Extended() 
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(extendImg.DOFade(1f, 0.1f));
        seq.AppendInterval(2f);
        seq.Append(extendImg.DOFade(0f,0.1f));
    }
    public int GetLifeCount() => lifeCount;
    public int GetFragmentCount() => fragmentCount;
}