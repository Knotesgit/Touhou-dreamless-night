using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    [Header("Lines")]
    public TextMeshProUGUI[] lines;

    [Header("Timings")]
    public float fadeInDuration = 0.8f;   // ���е���ʱ��
    public float lineStagger = 0.35f;     // �������еĵ�����
    public float holdDuration = 1.2f;     // ȫ�����ֺ�ͣ��
    public float fadeOutDuration = 0.8f;  // ���嵭��ʱ��

    [Header("Navigation")]
    public string titleSceneName = "Title"; // ��ı��ⳡ����
    public bool allowSkip = true;           // ����������
    public float minShowTime = 0.8f;        // ����չʾ��ú����������

    CanvasGroup group;
    float startTime;
    bool goingOut;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (!group) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        // ͳһ���ı�͸��
        if (lines != null)
        {
            foreach (var t in lines)
            {
                if (t == null) continue;
                var c = t.color;
                c.a = 0f;
                t.color = c;
            }
        }
    }

    void Start()
    {
        startTime = Time.time;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // ���ε���
        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var t = lines[i];
                if (t == null) continue;
                t.DOFade(1f, fadeInDuration).SetDelay(i * lineStagger);
            }
        }

        float totalInTime = ((lines == null || lines.Length == 0) ? 0f
            : (lines.Length - 1) * lineStagger + fadeInDuration);

        yield return new WaitForSeconds(totalInTime + holdDuration);

        // ���嵭��
        yield return FadeOutAndGotoTitle();
    }

    void Update()
    {
        if (!allowSkip) return;
        if (goingOut) return;
        if (Time.time - startTime < minShowTime) return;

        if (Input.anyKeyDown)
        {
            // ɱ������ tween��ֱ�ӿ�ʼ����
            DOTween.Kill(this, complete: false);
            if (lines != null)
                foreach (var t in lines) if (t) DOTween.Kill(t, false);
            StartCoroutine(FadeOutAndGotoTitle());
        }
    }

    IEnumerator FadeOutAndGotoTitle()
    {
        goingOut = true;
        // �� CanvasGroup �����嵭����û�о��˻�Ϊÿ�е���
        if (group)
        {
            var tw = group.DOFade(0f, fadeOutDuration);
            yield return tw.WaitForCompletion();
        }
        else
        {
            if (lines != null)
            {
                foreach (var t in lines)
                    if (t) t.DOFade(0f, fadeOutDuration);
            }
            yield return new WaitForSeconds(fadeOutDuration);
        }

        SceneManager.LoadScene(titleSceneName);
    }
}
