using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class TitleAttractController : MonoBehaviour
{
    [Header("Idle �� Play")]
    public float idleSeconds = 30f;
    public CanvasGroup titleUI;                // ��Ϊ null

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;                // ��ʾ��Ƶ����� RawImage
    public RenderTexture videoRT;              // VideoPlayer �� RT
    public bool stopOnAnyInput = true;

    [Header("Audio")]
    public AudioSource videoAudio;             // ר����Դ
    public float audioFadeIn = 0.5f;          // *��ʹ�ú�Ļ������ blackFadeTime ���Ǳ���ͬ��*
    public float audioFadeOut = 0.4f;         // *��ʹ�ú�Ļ������ blackFadeTime ���Ǳ���ͬ��*

    [Header("Visual Fades")]
    public float videoFadeIn = 0.35f;         // RawImage ���� ��
    public float videoFadeOut = 0.3f;

    [Header("Overlay")]
    public TMP_Text demoPlayLabel;             // ���� ��DEMO PLAY��
    public float demoBlinkPeriod = 0.8f;       // һ�ε���򵭳�ʱ��

    [Header("Extra Fade Overlay")]
    public CanvasGroup blackFade;              // ȫ����Ļ����ѡ��
    public float blackFadeTime = 0.5f;

    [Header("Timing")]
    public bool useUnscaledTime = true;        // �����߼��� unscaled�����뵭��Ҳ���� unscaled

    float lastInputTime;
    bool inAttractMode;
    Vector3 lastMousePos;
    bool hadFocusLastFrame = true;

    CanvasGroup videoCG;       // �� videoImage �ҵ� CanvasGroup
    float audioOrigVolume = 1; // ��¼ԭʼ����
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
            videoAudio.volume = 0f; // ������ 0 ����
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, videoAudio);
        }

        // �� RawImage ׼�� CanvasGroup �� �� ���뵭��
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

        // �¼����ģ������ظ���
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

        ResetIdleTimer();
        lastMousePos = Input.mousePosition;
    }

    void OnDestroy()
    {
        // �¼�ע�� & ���� Tween
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

        // --- �ؼ����ڶ��β��ž����Ѿ��� prepared ״̬ ---
        DOTween.Kill(videoAudio);
        if (videoAudio != null)
            videoAudio.volume = 0f;

        // ���õ���ͷ������д�����ɣ��е�ƽ̨ prefer frame��
        videoPlayer.time = 0;
        videoPlayer.frame = 0;

        if (videoPlayer.isPrepared)
        {
            // �Ѿ� prepared ��ֱ���������߼�
            OnVideoPrepared(videoPlayer);
        }
        else
        {
            videoPlayer.Prepare();

            // ĳЩƽ̨�� Prepare() ������ɵ��������ص�����������Ӹ�����
            if (videoPlayer.isPrepared)
                OnVideoPrepared(videoPlayer);
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // ������ʼ����
        TitleMenuController.Instance.setCanControl(false);
        vp.Play();
        if (videoAudio != null && !videoAudio.isPlaying) videoAudio.Play();

        // ���浭�루��Ļ��
        if (blackFade != null)
            blackFade.DOFade(0f, blackFadeTime).SetUpdate(useUnscaledTime);

        // �������루���Ļ�ϸ�ͬ����
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

        // DEMO PLAY ����
        if (demoPlayLabel != null)
        {
            demoBlinkTween?.Kill();
            demoPlayLabel.DOFade(0f, 0.15f)
                         .SetUpdate(useUnscaledTime)
                         .OnComplete(() => demoPlayLabel.gameObject.SetActive(false));
        }

        // ���� + ���� ͬ�������������Ժ�Ļʱ��Ϊ׼��
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
            // û�к�Ļ�����ٰ� RawImage ����
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

        // ���� UI ����
        if (titleUI != null)
        {
            titleUI.alpha = 0f;
            titleUI.DOFade(1f, 1f);
        }
        TitleMenuController.Instance.setCanControl(true);
        

        ResetIdleTimer();
    }
}
