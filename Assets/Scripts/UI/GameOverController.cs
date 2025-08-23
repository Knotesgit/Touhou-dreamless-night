using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI[] optionTexts;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.red;
    public RectTransform optionsParentTransform;

    private int currentIndex = 0;
    private bool canControl = false;
    private Vector3 gameOverTextStartPos;
    private Vector3 optionsStartPos;
    private PauseMenuController pauseMenuController;

    void Start()
    {
        pauseMenuController = GameObject.Find("PasueController").GetComponent<PauseMenuController>();
        gameOverTextStartPos = gameOverText.rectTransform.anchoredPosition;
        optionsStartPos = optionsParentTransform.anchoredPosition;
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOverMenu()
    {
        pauseMenuController.enabled = false;
        gameOverPanel.SetActive(true);
        canControl = false;

        // 动画显示文字
        RectTransform rt = gameOverText.rectTransform;
        rt.anchoredPosition = gameOverTextStartPos + new Vector3(0f, 30f, 0f);
        rt.DOAnchorPos(gameOverTextStartPos, 0.2f).SetUpdate(true);
        Color c = gameOverText.color;
        c.a = 0f;
        gameOverText.color = c;
        gameOverText.DOFade(1f, 0.2f).SetUpdate(true);

        // 动画显示选项
        RectTransform optionsRT = optionsParentTransform;
        optionsRT.anchoredPosition = optionsStartPos + new Vector3(50f, 0f, 0f);
        optionsRT.DOAnchorPos(optionsStartPos, 0.2f).SetUpdate(true);

        foreach (var text in optionTexts)
        {
            Color tc = text.color;
            tc.a = 0f;
            text.color = tc;
            text.DOFade(1f, 0.2f).SetUpdate(true);
        }

        HighlightOption(0);
        StartCoroutine(EnableControlAfterDelay());
    }

    IEnumerator EnableControlAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        canControl = true;
    }

    void Update()
    {
        if (!canControl) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeOption(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeOption(1);
        if (Input.GetKeyDown(KeyCode.Z)) ConfirmOption();
    }

    void ChangeOption(int dir)
    {
        currentIndex = (currentIndex + dir + optionTexts.Length) % optionTexts.Length;
        HighlightOption(currentIndex);
    }

    void HighlightOption(int index)
    {
        for (int i = 0; i < optionTexts.Length; i++)
            optionTexts[i].color = (i == index) ? selectedColor : normalColor;
    }

    void ConfirmOption()
    {
        switch (currentIndex)
        {
            case 0: LoadTitle(); break;
            case 1: Retry(); break;
            case 2: Application.Quit(); break;
        }
    }

    void Retry()
    {
        Time.timeScale = 1f;
        SwipeTransition.TransitionToScene("GameScene");
        canControl = false;
    }

    void LoadTitle()
    {
        Time.timeScale = 1f;
        SwipeTransition.TransitionToScene("TitleScene");
        canControl = false;
    }
}
