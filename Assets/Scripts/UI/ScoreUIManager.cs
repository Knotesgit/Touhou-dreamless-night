using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreUIManager : MonoBehaviour
{
    public static ScoreUIManager Instance { get; private set; }

    public TMP_Text scoreText;

    private int actualScore = 0;
    private int displayedScore = 0;
    private Tween scoreTween;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddScore(int delta)
    {
        actualScore += delta;

        if (scoreTween != null && scoreTween.IsActive()) scoreTween.Kill();

        scoreTween = DOTween.To(() => displayedScore, x =>
        {
            displayedScore = x;
            scoreText.text = displayedScore.ToString("D9");
        }, actualScore, 0.5f)
        .SetEase(Ease.OutCubic);
    }

    public int GetScore()
    {
        return actualScore;
    }
}
