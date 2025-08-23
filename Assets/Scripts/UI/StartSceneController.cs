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
    public float fadeInDuration = 0.8f;   // 单行淡入时间
    public float lineStagger = 0.35f;     // 相邻两行的淡入间隔
    public float holdDuration = 1.2f;     // 全部出现后停留
    public float fadeOutDuration = 0.8f;  // 整体淡出时间

    [Header("Navigation")]
    public string titleSceneName = "Title"; // 你的标题场景名
    public bool allowSkip = true;           // 允许按键跳过
    public float minShowTime = 0.8f;        // 最少展示多久后才允许跳过

    CanvasGroup group;
    float startTime;
    bool goingOut;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (!group) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        // 统一把文本透明
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
        // 依次淡入
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

        // 整体淡出
        yield return FadeOutAndGotoTitle();
    }

    void Update()
    {
        if (!allowSkip) return;
        if (goingOut) return;
        if (Time.time - startTime < minShowTime) return;

        if (Input.anyKeyDown)
        {
            // 杀掉所有 tween，直接开始淡出
            DOTween.Kill(this, complete: false);
            if (lines != null)
                foreach (var t in lines) if (t) DOTween.Kill(t, false);
            StartCoroutine(FadeOutAndGotoTitle());
        }
    }

    IEnumerator FadeOutAndGotoTitle()
    {
        goingOut = true;
        // 用 CanvasGroup 做整体淡出；没有就退化为每行淡出
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
