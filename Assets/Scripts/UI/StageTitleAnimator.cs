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
    public float waitTimeBeforeFadeOut = 2f; // ͣ��ʱ��

    private CanvasGroup cg;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();

        // 2. TitleName ����
        titleName1.GetComponent<CanvasGroup>().DOFade(1f, titleNameFadeDuration).SetDelay(0.5f);
        titleName2.GetComponent<CanvasGroup>().DOFade(1f, titleNameFadeDuration).SetDelay(0.5f);

        // 3. ��������
        background.GetComponent<CanvasGroup>().DOFade(1f, backgroundFadeDuration).SetDelay(barAnimationDuration + titleNameFadeDuration);

        // 4. TitleDescribe ����
        titleDescribe.GetComponent<CanvasGroup>().DOFade(1f, titleDescribeFadeDuration).SetDelay(barAnimationDuration + titleNameFadeDuration + backgroundFadeDuration)
            .OnComplete(() =>
            {
                // �ȴ������Ӻ��ٿ�ʼ����
                DOTween.Sequence()
                    .AppendInterval(waitTimeBeforeFadeOut) // �ȴ�һ��ʱ��
                    .Append(cg.DOFade(0f, 0.5f)) // ����
                    .OnComplete(() => Destroy(this)); // ���ٵ�ǰ�ű�
            });
    }
}

