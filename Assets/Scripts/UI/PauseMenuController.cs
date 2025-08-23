using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;


public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public TextMeshProUGUI pausingText;
    public TextMeshProUGUI[] optionTexts;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.red;
    public RectTransform optionsParentTransform;

    private int currentIndex = 0;
    private bool isPaused = false;
    private bool canControl = false;
    private Vector3 pauseTextStartPos;
    private Vector3 optionsStartPos;


    private void Start()
    {
        pauseTextStartPos = pausingText.rectTransform.anchoredPosition;
        optionsStartPos = optionsParentTransform.anchoredPosition;

        pausePanel.SetActive(false);
        StartCoroutine(DelayControl());
        
    }
    IEnumerator DelayControl()
    {

        yield return new WaitForSeconds(0.5f);
        canControl = true;
    }

    void Update()
    {
        if (!canControl) { return; }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                EnterPause();
            else
                ResumeGame();
        }

        if (!isPaused) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeOption(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeOption(1);
        if (Input.GetKeyDown(KeyCode.Z)) ConfirmOption();
    }

    void EnterPause()
    {
        GeneralAudioPool.Instance.PauseAll();
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        RectTransform rt = pausingText.rectTransform;
        rt.anchoredPosition = pauseTextStartPos + new Vector3(0f, 30f, 0f);
        rt.DOAnchorPos(pauseTextStartPos, 0.1f).SetUpdate(true);
        Color c = pausingText.color;
        c.a = 0f;
        pausingText.color = c;
        pausingText.DOFade(1f, 0.1f).SetUpdate(true);
        

        RectTransform optionsRT = optionsParentTransform;
        optionsRT.anchoredPosition = optionsStartPos + new Vector3(50f, 0f, 0f);
        optionsRT.DOAnchorPos(optionsStartPos, 0.1f).SetUpdate(true);
        foreach (var text in optionTexts)
        {
            Color tc = text.color;
            tc.a = 0f;
            text.color = tc;

            text.DOFade(1f, 0.1f).SetUpdate(true);
        }

        HighlightOption(0);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausingText.rectTransform.DOAnchorPos(pauseTextStartPos + new Vector3(0f, 30f, 0f), 0.1f).SetUpdate(true);
        pausingText.DOFade(0f, 0.1f).SetUpdate(true);
        foreach (var text in optionTexts)
        {
            text.DOFade(0f, 0.1f).SetUpdate(true);
        }
        optionsParentTransform.DOAnchorPos(optionsStartPos + new Vector3(50f, 0f, 0f), 0.1f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1f;
                GeneralAudioPool.Instance.ResumeAll();
            });

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
            case 0: ResumeGame(); break;
            case 1: LoadTitle(); break;
            case 2: Restart(); break;
            case 3: Application.Quit(); ; break;
        }
    }

    void LoadTitle()
    {
        Time.timeScale = 1f;
        SwipeTransition.TransitionToScene("TitleScene");
        canControl = false;
    }
    void Restart()
    {
        Time.timeScale = 1f;
        SwipeTransition.TransitionToScene("GameScene");
        canControl = false;
    }
}
