using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting; // 若没用新输入系统，可删掉，并改成 Input.GetKeyDown(KeyCode.Z)

public class EndingDirector : MonoBehaviour
{
    [Header("Invoke / Flow")]
    public bool autoTriggerOnStart = false;
    private bool endingRunning = false;

    [Header("Intro Object (step 1-3)")]
    public GameObject introPrefab;                 // 里面是若干 TextMeshPro（3D）/ SpriteRenderer
    public Transform introParent;                  // 可空
    public float waitingTimeToStart = 4f;
    public float introFadeInDuration = 0.6f;
    public float introIdleTime = 2.0f;             // 可按 Z 跳过
    public float introFadeOutDuration = 0.5f;

    [Header("Background Fade (with intro fade-out)")]
    public List<SpriteRenderer> bgSpriteRenderers; // 需要同时淡出的背景
    public float backgroundFadeOutDuration = 0.6f;

    [Header("Music Fade")]
    public float musicFadeOutDuration = 1.2f;      // 交给 GeneralAudioPool

    [Header("Texts Group (step 5-6)")]
    public TextMeshPro text;                // 结局文本（3D TMP）
    public TextMeshPro score;
    public TextMeshPro press;
    public float textFadeInDuration = 0.6f;
    public float textIdleTime = 2.0f;             // 可按 Z 跳过
    public float textFadeOutDuration = 0.5f;

    [Header("Sorting")]
    public string sortingLayerName = "EBullet";
    public int sortingOrder = 100;
    public bool forceSetSorting = true;            // 为 intro & texts 统一设置 sorting

    [Header("Exit")]
    public string nextSceneName = "Title";
    public float afterLoadDelay = 0f;

    // ---- 内部 ----
    GameObject introInstance;
    readonly List<Tween> runningTweens = new List<Tween>();

    void Start()
    {
        if (autoTriggerOnStart) TriggerEnding();
    }

    void OnDisable() => KillAllTweens();

    public void TriggerEnding()
    {
        if (endingRunning) return;
        endingRunning = true;
        StartCoroutine(RunEnding());
    }

    IEnumerator RunEnding()
    {
        // ===== 1) 实例化 intro 并淡入 =====
        List<TextMeshPro> introTMPs = null;
        List<SpriteRenderer> introSRs = null;
        yield return new WaitForSeconds(waitingTimeToStart);
        if (introPrefab != null)
        {
            introInstance = Instantiate(introPrefab, introParent ? introParent : null);

            introTMPs = new List<TextMeshPro>(introInstance.GetComponentsInChildren<TextMeshPro>(true));
            introSRs = new List<SpriteRenderer>(introInstance.GetComponentsInChildren<SpriteRenderer>(true));

            if (forceSetSorting)
            {
                foreach (var t in introTMPs) SetSorting(t, sortingLayerName, sortingOrder);
                foreach (var s in introSRs) SetSorting(s, sortingLayerName, sortingOrder);
            }

            // 置 0 再淡入
            SetAlpha(introTMPs, 0f);
            SetAlpha(introSRs, 0f);
            FadeTo(introTMPs, 1f, introFadeInDuration);
            FadeTo(introSRs, 1f, introFadeInDuration);

            yield return new WaitForSeconds(introFadeInDuration);
            // 可跳过等待
            yield return WaitSkippable(introIdleTime);
        }

        // ===== 2) intro & 背景一起淡出 =====
        {
            FadeTo(introTMPs, 0f, introFadeOutDuration);
            FadeTo(introSRs, 0f, introFadeOutDuration);
            FadeTo(bgSpriteRenderers, 0f, backgroundFadeOutDuration);
            // ===== 3) 全音乐淡出 =====
            yield return FadeOutAllAudio(musicFadeOutDuration);

            yield return new WaitForSeconds(Mathf.Max(introFadeOutDuration, backgroundFadeOutDuration));
        }

        

        // ===== 4) 结局文本整体淡入 → 可跳过 → 淡出 =====
        if (text != null)
        {
            if (forceSetSorting)
            {
                SetSorting(text, sortingLayerName, sortingOrder);
                SetSorting(score, sortingLayerName, sortingOrder);
                SetSorting(press, sortingLayerName, sortingOrder);
            }

            text.alpha = 0f;
            score.alpha = 0f;
            press.alpha = 0f;


            text.text = "Your score\n" +
                        "Score\n" +
                        "Card Bonus\n" +
                        "Miss\n" +
                        "Bomb Used\n\n\n";

            score.text =$"{PlayerScore.score}\n" +
                        $"{PlayerScore.numOfCardBonusGet}/6\n" +
                        $"{PlayerScore.numOfMiss}\n" +
                        $"{PlayerScore.numOFBombUsed}";
            press.text = "Press Z to proceed";

            text.DOFade(1f, textFadeInDuration);
            score.DOFade(1f, textFadeInDuration);
            press.DOFade(1f, textFadeInDuration);
            yield return new WaitForSeconds(textFadeInDuration);
            PlayerScore.reset();
            yield return WaitSkippable(textIdleTime);

            text.DOFade(0f, textFadeOutDuration);
            yield return new WaitForSeconds(textFadeOutDuration);
        }

        // ===== 5) Load Scene =====
        if (afterLoadDelay > 0f) yield return new WaitForSeconds(afterLoadDelay);
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // ================= 工具区 =================

    IEnumerator WaitSkippable(float waitSeconds)
    {
        if (waitSeconds <= 0f) yield break;
        float t = 0f;
        while (t < waitSeconds)
        {
            bool zPressed =
            #if ENABLE_INPUT_SYSTEM
                Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame;
            #else
                Input.GetKeyDown(KeyCode.Z);
            #endif
            if (zPressed) break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeOutAllAudio(float duration)
    {
        if (duration <= 0f) duration = 0.01f;
        // 你已有的全局音频池
        GeneralAudioPool.Instance.FadeOutAll(duration);
        yield return new WaitForSeconds(duration);
    }

    // --- Sorting ---
    void SetSorting(TextMeshPro tmp, string layer, int order)
    {
        if (!tmp) return;
        var r = tmp.GetComponent<MeshRenderer>();
        if (r)
        {
            r.sortingLayerName = layer;
            r.sortingOrder = order;
        }
    }

    void SetSorting(SpriteRenderer sr, string layer, int order)
    {
        if (!sr) return;
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
    }

    // --- Alpha 设置 ---
    void SetAlpha(IEnumerable<TextMeshPro> tmps, float a)
    {
        if (tmps == null) return;
        a = Mathf.Clamp01(a);
        foreach (var t in tmps)
        {
            if (!t) continue;
            var c = t.color;
            c.a = a;
            t.color = c;
            t.ForceMeshUpdate();
        }
    }

    void SetAlpha(IEnumerable<SpriteRenderer> srs, float a)
    {
        if (srs == null) return;
        a = Mathf.Clamp01(a);
        foreach (var s in srs)
        {
            if (!s) continue;
            var c = s.color;
            c.a = a;
            s.color = c;
        }
    }

    // --- Tween 组 ---
    void FadeTo(IEnumerable<TextMeshPro> tmps, float to, float duration)
    {
        if (tmps == null) return;
        foreach (var t in tmps)
        {
            if (!t) continue;
            var tw = t.DOFade(to, duration);
            PushTween(tw);
        }
    }

    void FadeTo(IEnumerable<SpriteRenderer> srs, float to, float duration)
    {
        if (srs == null) return;
        foreach (var s in srs)
        {
            if (!s) continue;
            var tw = s.DOFade(to, duration);
            PushTween(tw);
        }
    }

    void PushTween(Tween t)
    {
        if (t == null) return;
        t.SetUpdate(true); // 忽略 TimeScale（需要随 timescale 走就删掉）
        runningTweens.Add(t);
    }

    void KillAllTweens()
    {
        foreach (var t in runningTweens)
        {
            if (t != null && t.IsActive()) t.Kill();
        }
        runningTweens.Clear();
    }
}
