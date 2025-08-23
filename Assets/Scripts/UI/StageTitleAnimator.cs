using UnityEngine;
using DG.Tweening;

public class StageTitleAnimator : MonoBehaviour
{
    public GameObject titleName1;
    public GameObject titleName2;
    public GameObject background;
    public GameObject titleDescribe;

    public float barAnimationDuration = 1f;
    public float titleNameFadeDuration = 0.5f;
    public float backgroundFadeDuration = 1f;
    public float titleDescribeFadeDuration = 0.5f;
    public float waitTimeBeforeFadeOut = 2f; // 停留时间

    private CanvasGroup cg;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();

        // 2. TitleName 淡入
        titleName1.GetComponent<CanvasGroup>().DOFade(1f, titleNameFadeDuration).SetDelay(0.5f);
        titleName2.GetComponent<CanvasGroup>().DOFade(1f, titleNameFadeDuration).SetDelay(0.5f);

        // 3. 背景淡入
        background.GetComponent<CanvasGroup>().DOFade(1f, backgroundFadeDuration).SetDelay(barAnimationDuration + titleNameFadeDuration);

        // 4. TitleDescribe 淡入
        titleDescribe.GetComponent<CanvasGroup>().DOFade(1f, titleDescribeFadeDuration).SetDelay(barAnimationDuration + titleNameFadeDuration + backgroundFadeDuration)
            .OnComplete(() =>
            {
                // 等待几秒钟后再开始淡出
                DOTween.Sequence()
                    .AppendInterval(waitTimeBeforeFadeOut) // 等待一段时间
                    .Append(cg.DOFade(0f, 0.5f)) // 淡出
                    .OnComplete(() => Destroy(this)); // 销毁当前脚本
            });
    }
}

