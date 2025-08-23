using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class TitleAttractController : MonoBehaviour
{
    [Header("Idle → Play")]
    public float idleSeconds = 30f;
    public CanvasGroup titleUI;                // 可为 null

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;                // 显示视频画面的 RawImage
    public RenderTexture videoRT;              // VideoPlayer 的 RT
    public bool stopOnAnyInput = true;

    [Header("Audio")]
    public AudioSource videoAudio;             // 专用音源
    public float audioFadeIn = 0.5f;          // *若使用黑幕，将被 blackFadeTime 覆盖保持同步*
    public float audioFadeOut = 0.4f;         // *若使用黑幕，将被 blackFadeTime 覆盖保持同步*

    [Header("Visual Fades")]
    public float videoFadeIn = 0.35f;         // RawImage 自身 α
    public float videoFadeOut = 0.3f;

    [Header("Overlay")]
    public TMP_Text demoPlayLabel;             // 顶部 “DEMO PLAY”
    public float demoBlinkPeriod = 0.8f;       // 一次淡入或淡出时间

    [Header("Extra Fade Overlay")]
    public CanvasGroup blackFade;              // 全屏黑幕（可选）
    public float blackFadeTime = 0.5f;

    [Header("Timing")]
    public bool useUnscaledTime = true;        // 闲置逻辑走 unscaled，淡入淡出也可走 unscaled

    float lastInputTime;
    bool inAttractMode;
    Vector3 lastMousePos;
    bool hadFocusLastFrame = true;

    CanvasGroup videoCG;       // 给 videoImage 挂的 CanvasGroup
    float audioOrigVolume = 1; // 记录原始音量
    Tween demoBlinkTween;

    void Awake()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        if (videoRT != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRT;
        }

        if (videoAudio != null)
        {
            audioOrigVolume = videoAudio.volume;
            videoAudio.volume = 0f; // 进场从 0 淡入
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, videoAudio);
        }

        // 给 RawImage 准备 CanvasGroup 做 α 淡入淡出
        if (videoImage != null)
        {
            videoCG = videoImage.GetComponent<CanvasGroup>();
            if (videoCG == null) videoCG = videoImage.gameObject.AddComponent<CanvasGroup>();
            videoCG.alpha = 0f;
            videoImage.gameObject.SetActive(false);
        }

        if (demoPlayLabel != null)
        {
            var c = demoPlayLabel.color; c.a = 0f; demoPlayLabel.color = c;
            demoPlayLabel.gameObject.SetActive(false);
        }

        if (blackFade != null) blackFade.alpha = 0f;

        // 事件订阅（避免重复）
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

        ResetIdleTimer();
        lastMousePos = Input.mousePosition;
    }

    void OnDestroy()
    {
        // 事件注销 & 清理 Tween
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
        demoBlinkTween?.Kill();
        if (videoAudio != null) DOTween.Kill(videoAudio);
        if (blackFade != null) DOTween.Kill(blackFade);
        if (videoCG != null) DOTween.Kill(videoCG);
        if (titleUI != null) DOTween.Kill(titleUI);
        if (demoPlayLabel != null) DOTween.Kill(demoPlayLabel);
    }

    void Update()
    {

        if (!Application.isFocused) { hadFocusLastFrame = false; ResetIdleTimer(); return; }
        else if (!hadFocusLastFrame) { hadFocusLastFrame = true; ResetIdleTimer(); }

        if (DetectAnyInputThisFrame())
        {
            ResetIdleTimer();
            if (inAttractMode && stopOnAnyInput) StopAttractMode();
        }

        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (!inAttractMode && (now - lastInputTime) >= idleSeconds)
            StartAttractMode();
    }

    bool DetectAnyInputThisFrame()
    {
        if (Input.anyKeyDown) return true;
        var m = Input.mousePosition;
        if ((m - lastMousePos).sqrMagnitude > 0.1f) { lastMousePos = m; return true; }
        lastMousePos = m;
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f) return true;
        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f) return true;
        if (Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0.01f) return true;
        return false;
    }

    void ResetIdleTimer() => lastInputTime = useUnscaledTime ? Time.unscaledTime : Time.time;

    void StartAttractMode()
    {
        inAttractMode = true;

        titleUI?.DOFade(0f, 0.25f);

        if (videoImage != null)
        {
            videoImage.gameObject.SetActive(true);
            videoCG?.DOFade(1f, videoFadeIn).From(0f).SetUpdate(useUnscaledTime);
        }

        if (demoPlayLabel != null)
        {
            demoPlayLabel.gameObject.SetActive(true);
            demoBlinkTween?.Kill();
            demoBlinkTween = demoPlayLabel
                .DOFade(1f, demoBlinkPeriod).From(0f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        // --- 关键：第二次播放经常已经是 prepared 状态 ---
        DOTween.Kill(videoAudio);
        if (videoAudio != null)
            videoAudio.volume = 0f;

        // 重置到开头（两种写法都可；有的平台 prefer frame）
        videoPlayer.time = 0;
        videoPlayer.frame = 0;

        if (videoPlayer.isPrepared)
        {
            // 已经 prepared 就直接走启动逻辑
            OnVideoPrepared(videoPlayer);
        }
        else
        {
            videoPlayer.Prepare();

            // 某些平台上 Prepare() 立即完成但不触发回调；保险起见加个兜底
            if (videoPlayer.isPrepared)
                OnVideoPrepared(videoPlayer);
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // 真正开始播放
        TitleMenuController.Instance.setCanControl(false);
        vp.Play();
        if (videoAudio != null && !videoAudio.isPlaying) videoAudio.Play();

        // 画面淡入（黑幕）
        if (blackFade != null)
            blackFade.DOFade(0f, blackFadeTime).SetUpdate(useUnscaledTime);

        // 音量淡入（与黑幕严格同步）
        if (videoAudio != null)
        {
            DOTween.Kill(videoAudio);
            videoAudio.volume = 0f;
            float dur = blackFade != null ? blackFadeTime : Mathf.Max(0.01f, audioFadeIn);
            DOTween.To(() => videoAudio.volume, v => videoAudio.volume = v, audioOrigVolume, dur)
                   .SetUpdate(useUnscaledTime);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StopAttractMode();
    }

    public void StopAttractMode()
    {
        if (!inAttractMode) return;
        inAttractMode = false;

        // DEMO PLAY 淡出
        if (demoPlayLabel != null)
        {
            demoBlinkTween?.Kill();
            demoPlayLabel.DOFade(0f, 0.15f)
                         .SetUpdate(useUnscaledTime)
                         .OnComplete(() => demoPlayLabel.gameObject.SetActive(false));
        }

        // 画面 + 音量 同步淡出（优先以黑幕时间为准）
        float dur = blackFade != null ? blackFadeTime : Mathf.Max(0.01f, audioFadeOut);

        if (blackFade != null)
        {
            DOTween.Kill(blackFade);
            blackFade.DOFade(1f, dur)
                     .SetUpdate(useUnscaledTime)
                     .OnComplete(() =>
                     {
                         if (videoCG != null) videoCG.alpha = 0f;
                         if (videoImage != null) videoImage.gameObject.SetActive(false);
                         if (videoPlayer.isPlaying) videoPlayer.Stop();
                     });
        }
        else
        {
            // 没有黑幕，至少把 RawImage 淡出
            if (videoCG != null)
                videoCG.DOFade(0f, videoFadeOut).SetUpdate(useUnscaledTime)
                    .OnComplete(() => { if (videoImage != null) videoImage.gameObject.SetActive(false); });
            if (videoPlayer.isPlaying) videoPlayer.Stop();
        }

        if (videoAudio != null)
        {
            DOTween.Kill(videoAudio);
            DOTween.To(() => videoAudio.volume, v => videoAudio.volume = v, 0f, dur)
                   .SetUpdate(useUnscaledTime)
                   .OnComplete(() => 
                   { 
                       if (videoAudio.isPlaying) videoAudio.Stop();
                       blackFade.DOFade(0f, 0.25f);
                   });
        }

        // 标题 UI 回来
        if (titleUI != null)
        {
            titleUI.alpha = 0f;
            titleUI.DOFade(1f, 1f);
        }
        TitleMenuController.Instance.setCanControl(true);
        

        ResetIdleTimer();
    }
}
