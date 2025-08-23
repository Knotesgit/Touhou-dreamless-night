using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
public class TitleAnimator : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup menuPanel;
    public TextMeshProUGUI titleText;         // “东方不能寐”
    public TextMeshProUGUI moonText;          // “夜”
    public Image moonImage;                   // 月亮图（subTextYe）
    public Image moonRing;                    // 光圈图
    
    [Header("Animation Settings")]
    public float floatDistance = 30f;
    public static event Action OnAnimationComplete; // ← 加一个事件

    [Header("Audio Settings")]
    public AudioClip moonRingClip;




    private Vector2 titleStartPos;
    private Vector2 moonImageStartPos;
    private Vector3 ringOriginalScale;
    private Boolean skipped = false;

    private void Awake()
    {
        titleStartPos = titleText.rectTransform.anchoredPosition;
        moonImageStartPos = moonImage.rectTransform.anchoredPosition;
        ringOriginalScale = moonRing.transform.localScale;
    }
    void Start()
    {
        InitUIState();
        PlayTitleIntroSequence();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Z)&&!skipped)
        {
            SkipAnimation();
            skipped = true;
        }
    }
    void InitUIState()
    {
        titleText.alpha = 0f;
        moonText.alpha = 1f;
        SetAlpha(moonImage, 0f);
        SetAlpha(moonRing, 0f);

        menuPanel.alpha = 0f;
        menuPanel.interactable = false;
        menuPanel.blocksRaycasts = false;
    }

    void PlayTitleIntroSequence()
    {
        Sequence seq = DOTween.Sequence();

        // --- Title Text 入场 ---
        Vector2 titleStartPos = titleText.rectTransform.anchoredPosition - new Vector2(0, floatDistance);
        titleText.rectTransform.anchoredPosition = titleStartPos;
        seq.Append(titleText.DOFade(1f, 1f));
        seq.Join(titleText.rectTransform.DOAnchorPosY(titleStartPos.y + floatDistance, 1f).SetEase(Ease.OutQuad));
        seq.AppendCallback(StartLoopingEffectTitle);

        // --- Moon 入场 ---
        Vector2 moonStartPos = moonImage.rectTransform.anchoredPosition - new Vector2(0, floatDistance);
        moonImage.rectTransform.anchoredPosition = moonStartPos;

        seq.AppendInterval(0.8f);
        seq.Append(moonImage.rectTransform.DOAnchorPosY(moonStartPos.y + floatDistance, 2f).SetEase(Ease.OutQuad));
        seq.Join(moonImage.DOFade(1f, 2f));
        seq.Join(moonRing.DOFade(1f, 2f));

        // --- Ring 光波 ---
        Vector3 originalScale = moonRing.transform.localScale;
        seq.AppendCallback(() => GeneralAudioPool.Instance.PlayOneShot(moonRingClip));
        seq.Append(moonRing.transform.DOScale(originalScale * 3f, 1f).SetEase(Ease.OutSine));
        seq.Join(moonRing.DOFade(0f, 1f));
        seq.Append(moonRing.transform.DOScale(originalScale, 0.01f)); // 复原
        seq.Append(moonRing.DOFade(1f, 1f));

        // --- 开始循环动画 ---
        seq.AppendCallback(StartLoopingEffectMoon);

        // --- 菜单渐显 ---
        seq.AppendInterval(0.3f);
        seq.Append(menuPanel.DOFade(1f, 0.5f));
        seq.OnComplete(() => {
            menuPanel.interactable = true;
            menuPanel.blocksRaycasts = true;
            OnAnimationComplete?.Invoke();
        });
    }

    void StartLoopingEffectTitle()
    {
        titleText.DOKill();  // 保证只有一个 tween
        titleText.DOFade(0.3f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
        titleText.rectTransform.DOKill();
        titleText.rectTransform.DOAnchorPosY(titleText.rectTransform.anchoredPosition.y + 5f, 1f)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    void StartLoopingEffectMoon()
    {
        moonImage.DOKill();
        moonImage.rectTransform.DOKill();
        moonImage.rectTransform.DOAnchorPosY(moonImage.rectTransform.anchoredPosition.y + 5f, 1f)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);

        moonRing.DOKill();
        moonRing.DOFade(0.3f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    void SetAlpha(Graphic g, float a)
    {
        Color c = g.color;
        c.a = a;
        g.color = c;
    }

    void SkipAnimation()
    {
        DOTween.KillAll();

        // 恢复终态
        titleText.alpha = 1f;
        titleText.rectTransform.anchoredPosition = titleStartPos;

        moonImage.rectTransform.anchoredPosition = moonImageStartPos;
        SetAlpha(moonImage, 1f);

        SetAlpha(moonRing, 1f);
        moonRing.transform.localScale = ringOriginalScale;

        menuPanel.alpha = 1f;
        menuPanel.interactable = true;
        menuPanel.blocksRaycasts = true;

        StartLoopingEffectTitle();
        StartLoopingEffectMoon();

        OnAnimationComplete?.Invoke();

    }
}