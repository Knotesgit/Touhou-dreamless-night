using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombUIManager : MonoBehaviour
{
    [Header("UI Resources")]
    public Sprite emptyBombSprite;
    public Sprite fullBombSprite;
    public GameObject bombImagePrefab;
    public Transform bombPanel;
    public AudioClip bombExtend;

    [Header("Settings")]
    public int maxBombs = 7;
    public int bombCount = 0;
    private List<Image> bombImages = new List<Image>();
    

    void Start()
    {
        for (int i = 0; i < maxBombs; i++)
        {
            GameObject go = Instantiate(bombImagePrefab, bombPanel);
            go.transform.localScale = Vector3.one;
            Image img = go.GetComponent<Image>();
            img.sprite = emptyBombSprite;
            bombImages.Add(img);
        }
        UpdateBombUI();
    }

    public void SetBombCount(int count)
    {
        bombCount = Mathf.Clamp(count, 0, maxBombs);
        UpdateBombUI();
    }

    public void AddBomb()
    {
        if (bombCount < maxBombs)
        {
            bombCount++;
            UpdateBombUI();
            GeneralAudioPool.Instance.PlayOneShot(bombExtend);
        }
    }

    public void UseBomb()
    {
        if (bombCount > 0)
        {
            bombCount--;
            UpdateBombUI();
        }
    }

    private void UpdateBombUI()
    {
        for (int i = 0; i < maxBombs; i++)
        {
            bombImages[i].sprite = i < bombCount ? fullBombSprite : emptyBombSprite;
            bombImages[i].enabled = true;
        }
    }

    public int GetBombCount() => bombCount;
}