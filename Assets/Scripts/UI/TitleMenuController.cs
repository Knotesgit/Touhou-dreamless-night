using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;
using System.Collections;

public class TitleMenuController : MonoBehaviour
{
    public CanvasGroup mainMenuPanel;
    public CanvasGroup manualPanel;
    public TMP_Text[] mainMenuOptions;
    public TMP_Text[] manualOptions;
    public TMP_Text whatGame;
    public TMP_Text howToControl;
    public TMP_Text story;
    public static TitleMenuController Instance;

    private int currentIndex = 0;
    private bool inManual = false;
    private bool inManualText = false;
    private bool canControl = false;
    private Vector2[] mainMenuOriginalPositions;
    private Vector2[] manualOriginalPositions;
    private Vector2[] manualTextOriginalPositions;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.red;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        mainMenuOptions[0].color= selectedColor;
        mainMenuOriginalPositions = new Vector2[mainMenuOptions.Length];
        for (int i = 0; i < mainMenuOptions.Length; i++)
        {
            mainMenuOriginalPositions[i] = mainMenuOptions[i].rectTransform.anchoredPosition;
        }

        manualOriginalPositions = new Vector2[manualOptions.Length];
        for (int i = 0; i < manualOptions.Length; i++)
        {
            manualOriginalPositions[i] = manualOptions[i].rectTransform.anchoredPosition;
        }
        manualTextOriginalPositions = new Vector2[3];
        manualTextOriginalPositions[0] = whatGame.rectTransform.anchoredPosition;
        manualTextOriginalPositions[1] = howToControl.rectTransform.anchoredPosition;
        manualTextOriginalPositions[2] = story.rectTransform.anchoredPosition;
        SetAlpha(whatGame, 0f);
        SetAlpha(howToControl, 0f);
        SetAlpha(story, 0f);

    }

    void Update()
    {
        if (canControl) 
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeOption(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeOption(1);
            if (Input.GetKeyDown(KeyCode.Z)) Confirm();
            if (inManual && Input.GetKeyDown(KeyCode.X)) ReturnToMain();
            if (inManualText && Input.GetKeyDown(KeyCode.X)) ReturnToManual();
        }
    }

    void OnEnable()
    {
        TitleAnimator.OnAnimationComplete += EnableControl;
    }
    void OnDisable()
    {
        TitleAnimator.OnAnimationComplete -= EnableControl;
    }
    void EnableControl()
    {
        StartCoroutine(EnableControlNextFrame());
    }
    IEnumerator EnableControlNextFrame()
    {
        yield return null;  // 延迟到下一帧
        canControl = true;
    }
    void ChangeOption(int dir)
    {
        var options = inManual ? manualOptions : mainMenuOptions;
        currentIndex = (currentIndex + dir + options.Length) % options.Length;
        Highlight(options);
    }

    void Confirm()
    {
        canControl = false;
        if (inManual)
        {
            switch (currentIndex)
            {
                case 0: ShowText(whatGame); break;
                case 1: ShowText(howToControl); break;
                case 2: ShowText(story); break;
            }
            return;
        }

        switch (currentIndex)
        {
            case 0: SwipeTransition.TransitionToScene("GameScene"); break;
            case 1: ShowManual(); break;
            case 2: Application.Quit(); break;
        }
    }

    void ShowMainMenu()
    {
        canControl = false;
        mainMenuPanel.DOFade(1f, 0.5f).OnComplete(() => canControl = true);
        mainMenuPanel.interactable = true;
        mainMenuPanel.blocksRaycasts = true;

        manualPanel.DOFade(0f, 0.5f);
        manualPanel.interactable = false;
        manualPanel.blocksRaycasts = false;

        inManual = false;
        currentIndex = 0;
        Highlight(mainMenuOptions);
    }

    void ShowManual()
    {
        canControl = false;
        mainMenuPanel.DOFade(0f, 0.5f);
        mainMenuPanel.interactable = false;
        mainMenuPanel.blocksRaycasts = false;

        manualPanel.DOFade(1f ,0.5f).OnComplete(() => canControl = true);
        manualPanel.interactable = true;
        manualPanel.blocksRaycasts = true;

        inManual = true;
        currentIndex = 0;
        Highlight(manualOptions);
    }

    void ReturnToMain()
    {
        ShowMainMenu();
    }

    void ReturnToManual() 
    {
        inManualText = false;
        inManual=true;
        canControl = false;

        whatGame.DOFade(0f, 0.5f);
        whatGame.rectTransform.DOAnchorPosY(manualTextOriginalPositions[0].y,0.5f);
        howToControl.DOFade(0f, 0.5f);
        howToControl.rectTransform.DOAnchorPosY(manualTextOriginalPositions[0].y, 0.5f);
        story.DOFade(0f, 0.5f);
        story.rectTransform.DOAnchorPosY(manualTextOriginalPositions[0].y, 0.5f);
        foreach (var opt in manualOptions)
        {
            opt.DOFade(1f, 0.5f);
            opt.raycastTarget = true;
        }

        Highlight(manualOptions); // 恢复高亮
        canControl = true;
    }

    void Highlight(TMP_Text[] options)
    {
        Vector2[] originalPositions = (options == mainMenuOptions) ? mainMenuOriginalPositions : manualOriginalPositions;

        for (int i = 0; i < options.Length; i++)
        {
            var rt = options[i].rectTransform;

            // Kill 之前先手动归位（防止因 tween 中止留下偏移）
            SetX(rt, originalPositions[i].x);
            DOTween.Kill(options[i], id: "flash_" + i);
            DOTween.Kill(rt, id: "shake_" + i);

            // 设置颜色
            options[i].color = (i == currentIndex) ? selectedColor : normalColor;

            if (i == currentIndex)
            {
                int index = i;

                options[i].DOFade(0.3f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true)
                    .SetId("flash_" + i);

                rt.DOAnchorPosX(originalPositions[i].x + 10f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetId("shake_" + i);
            }
            else
            {
                SetAlpha(options[i], 1f);
            }
        }
    }
    void ShowText(TMP_Text target)
    {
        inManual = false;
        inManualText = true;
        canControl = false;

        // 隐藏选项
        foreach (var opt in manualOptions)
        {
            opt.DOFade(0f, 0.3f).OnComplete(() => opt.DOKill());
            
            opt.raycastTarget = false;
        }
        // 显示当前文本
        target.rectTransform.DOAnchorPosY(target.rectTransform.anchoredPosition.y + 200f, 0.5f);
        target.DOFade(1f, 0.5f).OnComplete(() => {
            canControl = true;
        });

        
        
    }
    void SetAlpha(Graphic g, float a)
    {
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
    void SetX(RectTransform rt, float x)
    {
        Vector2 pos = rt.anchoredPosition;
        pos.x = x;
        rt.anchoredPosition = pos;
    }

    public void setCanControl(bool c) 
    {
        canControl=c;
    }
}