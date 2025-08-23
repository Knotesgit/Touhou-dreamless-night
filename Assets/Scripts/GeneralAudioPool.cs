//using System.Collections.Generic;
//using UnityEngine;

//public class GeneralAudioPool : MonoBehaviour
//{
//    public static GeneralAudioPool Instance;
//    public GameObject audioPrefab;
//    public int poolSize = 10;

//    private List<AudioSource> audioSources;
//    private int index = 0;
//    private Dictionary<AudioClip, float> lastPlayTime = new();

//    [SerializeField] private int maxPoolSize = 1100;
//    [SerializeField] private float defaultCooldown = 0.03f;

//    void Awake()
//    {
//        if (Instance == null) 
//        { 
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }

//        audioSources = new List<AudioSource>();
//        for (int i = 0; i < poolSize; i++)
//        {
//            GameObject go = Instantiate(audioPrefab, transform);
//            go.name = $"AudioSource_{i}";
//            var source = go.GetComponent<AudioSource>();
//            go.SetActive(true); // 不关对象，保证音效能正常播完
//            audioSources.Add(source);
//        }
//    }

//    /// <summary>
//    /// 播放一个短音效，不影响其他播放中的音效（推荐用法）
//    /// </summary>
//    public void PlayOneShot(AudioClip clip, float volume = 1f, float minInterval = -1f)
//    {
//        if (clip == null) return;
//        if (minInterval < 0f) minInterval = defaultCooldown;

//        // 限频（避免刷同一个 clip）
//        if (lastPlayTime.TryGetValue(clip, out float lastTime))
//        {
//            if (Time.time - lastTime < minInterval)
//                return;
//        }
//        lastPlayTime[clip] = Time.time;

//        // 找一个空闲 AudioSource
//        for (int i = 0; i < audioSources.Count; i++)
//        {
//            int idx = (index + i) % audioSources.Count;
//            if (!audioSources[idx].isPlaying)
//            {
//                audioSources[idx].PlayOneShot(clip, volume);
//                index = (idx + 1) % audioSources.Count;
//                return;
//            }
//        }

//        // 所有都在播，允许临时创建（限制最大数量）
//        if (audioSources.Count < maxPoolSize)
//        {
//            GameObject go = Instantiate(audioPrefab, transform);
//            var source = go.GetComponent<AudioSource>();
//            source.PlayOneShot(clip, volume);
//            audioSources.Add(source);
//        }
//        // 超出池最大值：不播放 or 播个默认静音的 fallback（可选）
//    }

//    /// <summary>
//    /// 设置 clip 并播放（适用于长音效或 loop）
//    /// </summary>
//    public void Play(AudioClip clip, float volume = 1f, bool loop = false)
//    {
//        var source = audioSources[index];
//        source.Stop();
//        source.loop = loop;
//        source.clip = clip;
//        source.volume = volume;
//        source.Play();
//        index = (index + 1) % poolSize;
//    }
//    public AudioSource PlayAndReturnSource(AudioClip clip, float volume = 1f, bool loop = false)
//    {
//        var source = audioSources[index];
//        source.Stop();
//        source.loop = loop;
//        source.clip = clip;
//        source.volume = volume;
//        source.Play();
//        index = (index + 1) % audioSources.Count;
//        return source;
//    }
//    public void StopAll()
//    {
//        foreach (var source in audioSources)
//        {
//            source.Stop();
//            source.clip = null;
//        }
//    }

//    public void PauseAll()
//    {
//        foreach (var source in audioSources)
//        {
//            if (source.isPlaying)
//                source.Pause();
//        }
//    }

//    public void ResumeAll()
//    {
//        foreach (var source in audioSources)
//        {
//                source.UnPause();
//        }
//    }


//}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneralAudioPool : MonoBehaviour
{
    public static GeneralAudioPool Instance;
    public GameObject audioPrefab;
    public int poolSize = 10;

    private List<AudioSource> audioSources;
    private int index = 0;
    private Dictionary<AudioClip, float> lastPlayTime = new();
    private readonly Dictionary<AudioSource, Coroutine> _fadeRoutines = new();

    [SerializeField] private int maxPoolSize = 1100;
    [SerializeField] private float defaultCooldown = 0.03f;

    //用于标记一帧内重复播放的 clip（防止瞬间刷音）
    private HashSet<AudioClip> clipsThisFrame = new();

    //临时创建的 source，便于清理
    private Queue<AudioSource> dynamicSources = new();

    void Awake()
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

        audioSources = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(audioPrefab, transform);
            go.name = $"AudioSource_{i}";
            var source = go.GetComponent<AudioSource>();
            go.SetActive(true); // 不关对象，保证音效能正常播完
            audioSources.Add(source);
        }
    }

    void LateUpdate() //每帧清空限制播放列表
    {
        clipsThisFrame.Clear();

        //清理已播放完的动态 AudioSource
        while (dynamicSources.Count > 0)
        {
            var src = dynamicSources.Peek();
            if (!src.isPlaying)
            {
                Destroy(src.gameObject);
                audioSources.Remove(src);
                dynamicSources.Dequeue();
            }
            else break;
        }
    }

    /// <summary>
    /// 播放一个短音效，不影响其他播放中的音效（推荐用法）
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f, float minInterval = -1f)
    {
        if (clip == null) return;
        if (minInterval < 0f) minInterval = defaultCooldown;

        //防止一帧内重复播放相同 clip
        if (clipsThisFrame.Contains(clip)) return;
        clipsThisFrame.Add(clip);

        // 限频（避免刷同一个 clip）
        if (lastPlayTime.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < minInterval)
                return;
        }
        lastPlayTime[clip] = Time.time;

        // 找一个空闲 AudioSource
        for (int i = 0; i < audioSources.Count; i++)
        {
            int idx = (index + i) % audioSources.Count;
            if (!audioSources[idx].isPlaying)
            {
                var source = audioSources[idx];

                //改为明确赋值播放，避免 PlayOneShot 失控
                source.Stop();
                source.clip = clip;
                source.loop = false;
                source.volume = volume;
                source.Play();

                index = (idx + 1) % audioSources.Count;
                //Debug.Log($"[{Time.time:F2}s] Playing {clip.name} on source {source.name} with volume {source.volume}, priority {source.priority}");
                return;
            }
        }

        // 所有都在播，允许临时创建（限制最大数量）
        if (audioSources.Count < maxPoolSize)
        {
            GameObject go = Instantiate(audioPrefab, transform);
            var source = go.GetComponent<AudioSource>();

            source.clip = clip;
            source.loop = false;
            source.volume = volume;
            source.Play();

            go.SetActive(true);
            audioSources.Add(source);
            dynamicSources.Enqueue(source);
        }
        // 否则丢弃，不播放（保持静默）
    }

    /// <summary>
    /// 设置 clip 并播放（适用于长音效或 loop）
    /// </summary>
    public void Play(AudioClip clip, float volume = 1f, bool loop = false)
    {
        var source = audioSources[index];
        source.Stop();
        source.loop = loop;
        source.clip = clip;
        source.volume = volume;
        source.Play();
        index = (index + 1) % poolSize;
    }

    public AudioSource PlayAndReturnSource(AudioClip clip, float volume = 1f, bool loop = false)
    {
        var source = audioSources[index];
        source.Stop();
        source.loop = loop;
        source.clip = clip;
        source.volume = volume;
        source.Play();
        index = (index + 1) % audioSources.Count;
        return source;
    }

    public void StopAll()
    {
        foreach (var source in audioSources)
        {
            source.Stop();
            source.clip = null;
        }
    }

    public void PauseAll()
    {
        foreach (var source in audioSources)
        {
            if (source.isPlaying)
                source.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (var source in audioSources)
        {
            source.UnPause();
        }
    }

    public void FadeOutAll(float duration, bool onlyLoop = true, bool stopAtEnd = true)
    {
        if (duration <= 0f)
        {
            // 直接拉闸
            foreach (var src in audioSources)
            {
                if (!src || !src.isPlaying) continue;
                if (onlyLoop && !src.loop) continue;
                src.volume = 0f;
                if (stopAtEnd)
                {
                    src.Stop();
                    src.clip = null;
                }
            }
            return;
        }

        // 拷贝一份，避免迭代时集合变化
        var list = audioSources.ToArray();
        foreach (var src in list)
        {
            if (!src || !src.isPlaying) continue;
            if (onlyLoop && !src.loop) continue;

            // 已经在淡出，先停掉旧协程
            if (_fadeRoutines.TryGetValue(src, out var co) && co != null)
                StopCoroutine(co);

            var routine = StartCoroutine(Co_FadeOutSource(src, duration, stopAtEnd));
            _fadeRoutines[src] = routine;
        }
    }

    private IEnumerator Co_FadeOutSource(AudioSource src, float duration, bool stopAtEnd)
    {
        if (!src) yield break;

        float startVol = src.volume;
        float t = 0f;

        while (t < duration && src && (src.isPlaying || src.volume > 0f))
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            src.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        if (src)
        {
            src.volume = 0f;
            if (stopAtEnd)
            {
                src.Stop();
                src.clip = null;
            }
        }

        _fadeRoutines.Remove(src);
    }

    /// <summary>
    /// 取消所有正在进行的淡出，但不会恢复音量；仅停止协程。
    /// </summary>
    public void CancelAllFades()
    {
        foreach (var kv in _fadeRoutines.ToArray())
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
        }
        _fadeRoutines.Clear();
    }

}

