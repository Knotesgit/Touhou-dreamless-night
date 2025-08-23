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
//            go.SetActive(true); // ���ض��󣬱�֤��Ч����������
//            audioSources.Add(source);
//        }
//    }

//    /// <summary>
//    /// ����һ������Ч����Ӱ�����������е���Ч���Ƽ��÷���
//    /// </summary>
//    public void PlayOneShot(AudioClip clip, float volume = 1f, float minInterval = -1f)
//    {
//        if (clip == null) return;
//        if (minInterval < 0f) minInterval = defaultCooldown;

//        // ��Ƶ������ˢͬһ�� clip��
//        if (lastPlayTime.TryGetValue(clip, out float lastTime))
//        {
//            if (Time.time - lastTime < minInterval)
//                return;
//        }
//        lastPlayTime[clip] = Time.time;

//        // ��һ������ AudioSource
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

//        // ���ж��ڲ���������ʱ�������������������
//        if (audioSources.Count < maxPoolSize)
//        {
//            GameObject go = Instantiate(audioPrefab, transform);
//            var source = go.GetComponent<AudioSource>();
//            source.PlayOneShot(clip, volume);
//            audioSources.Add(source);
//        }
//        // ���������ֵ�������� or ����Ĭ�Ͼ����� fallback����ѡ��
//    }

//    /// <summary>
//    /// ���� clip �����ţ������ڳ���Ч�� loop��
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

    //���ڱ��һ֡���ظ����ŵ� clip����ֹ˲��ˢ����
    private HashSet<AudioClip> clipsThisFrame = new();

    //��ʱ������ source����������
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
            go.SetActive(true); // ���ض��󣬱�֤��Ч����������
            audioSources.Add(source);
        }
    }

    void LateUpdate() //ÿ֡������Ʋ����б�
    {
        clipsThisFrame.Clear();

        //�����Ѳ�����Ķ�̬ AudioSource
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
    /// ����һ������Ч����Ӱ�����������е���Ч���Ƽ��÷���
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f, float minInterval = -1f)
    {
        if (clip == null) return;
        if (minInterval < 0f) minInterval = defaultCooldown;

        //��ֹһ֡���ظ�������ͬ clip
        if (clipsThisFrame.Contains(clip)) return;
        clipsThisFrame.Add(clip);

        // ��Ƶ������ˢͬһ�� clip��
        if (lastPlayTime.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < minInterval)
                return;
        }
        lastPlayTime[clip] = Time.time;

        // ��һ������ AudioSource
        for (int i = 0; i < audioSources.Count; i++)
        {
            int idx = (index + i) % audioSources.Count;
            if (!audioSources[idx].isPlaying)
            {
                var source = audioSources[idx];

                //��Ϊ��ȷ��ֵ���ţ����� PlayOneShot ʧ��
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

        // ���ж��ڲ���������ʱ�������������������
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
        // �������������ţ����־�Ĭ��
    }

    /// <summary>
    /// ���� clip �����ţ������ڳ���Ч�� loop��
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
            // ֱ����բ
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

        // ����һ�ݣ��������ʱ���ϱ仯
        var list = audioSources.ToArray();
        foreach (var src in list)
        {
            if (!src || !src.isPlaying) continue;
            if (onlyLoop && !src.loop) continue;

            // �Ѿ��ڵ�������ͣ����Э��
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
    /// ȡ���������ڽ��еĵ�����������ָ���������ֹͣЭ�̡�
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

