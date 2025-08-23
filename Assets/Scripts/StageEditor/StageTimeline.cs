using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StageTimeline : MonoBehaviour
{
    public StageData stageData;
    public BackgroundManager backgroundManager;
    public FogSpawner fogSpawner;
    public Canvas canvas;

    [Header("Editor View Filter (Not for runtime)")]
    public string editorFilterGroup = "";
    [Min(0f)]
    public float editorFilterStartTime = 0f;
    [Min(0f)]
    public float editorFilterEndTime = 9999f;

    private float timer;
    private bool wasFogOn = false;
    
    private List<StageEvent> allEvents = new();
    private int eventIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        allEvents.Clear();

        foreach (var e in stageData.enemyEvents)
            allEvents.Add(new EnemySpawnStageEvent { time = e.time, data = e });

        foreach (var e in stageData.backgroundEvents)
            allEvents.Add(new BackgroundStageEvent { time = e.time, data = e });

        foreach (var e in stageData.audioEvents)
            allEvents.Add(new AudioStageEvent { time = e.time, data = e });

        foreach (var e in stageData.displayEvents)
            allEvents.Add(new DisplayStageEvent { time = e.time, data = e });
        foreach (var e in stageData.bossEvents)
            allEvents.Add(new BossStageEvent{ time = e.time, data = e });

        allEvents.Sort((a, b) => a.time.CompareTo(b.time));
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        while (eventIndex < allEvents.Count && timer >= allEvents[eventIndex].time)
        {
            allEvents[eventIndex].Execute(this);
            eventIndex++;
        }
        //timer += Time.deltaTime;
        //CheckEnemySpawns();
        //CheckBackgroundChanges();
        //CheckAudioTriggers();
    }
   
    public void SpawnEnemy(EnemySpawnEvent e)
    {
        Vector2 spawnPos = e.spawnType == EnemySpawnEvent.SpawnType.FlyIn ? e.moveFrom : e.spawnPosition;
        GameObject enemy = PoolManager.Instance.Get(e.enemyPrefab, spawnPos);
        Enemy script = enemy.GetComponent<Enemy>();
        if (script != null) 
        {
            script.SetAcceleration(e.useAcceleration);
            script.SetTargetSpeed(e.tragetSpeed);
            script.SetAccDuration(e.accelerateDuration);
            script.SetDropItem(e.dropItem);
            script.SetHp(e.hp);
            script.SetStayDuration(e.stayDuration);
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            sr.color = Color.white;

            switch (e.spawnType)
            {
                case EnemySpawnEvent.SpawnType.Instant:
                    enemy.transform.position = e.spawnPosition;
                    script.SetMovementEnabled(true);
                    break;

                case EnemySpawnEvent.SpawnType.FadeIn:
                    enemy.transform.position = e.spawnPosition;
                    enemy.transform.localScale = Vector3.zero;
                    sr.color = new Color(1, 1, 1, 0);
                    enemy.transform.DOScale(1f, e.moveDuration);
                    sr.DOFade(1f, e.moveDuration).OnComplete(() => script.SetMovementEnabled(true));
                    break;

                case EnemySpawnEvent.SpawnType.FlyIn:
                    enemy.transform.position = e.moveFrom;
                    Vector3 mid = (e.moveFrom + e.spawnPosition) / 2 + Vector2.down * 0.5f;
                    Vector3[] path = new Vector3[] { e.moveFrom, mid, e.spawnPosition };

                    enemy.transform.DOPath(path, e.moveDuration, PathType.CatmullRom)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() => script.SetMovementEnabled(true));
                    break;
            }

        }
        GameObject patternObj = Instantiate(e.pattern.gameObject, enemy.transform);
        patternObj.transform.localPosition = Vector3.zero;
    }

    public void ApplyBackgroundEvent(BackgroundEvent e)
    {
        // �л�����ͼ�����״̬�������룩
        backgroundManager.CrossFadeToSprite(
            e.backgroundSprite,
            e.enableScroll,
            e.scrollSpeed,
            e.enableFade ? e.fadeDuration : 0.5f // Ĭ�ϵ���ʱ��
        );

        // ��������͸���ȣ��ɵ��ӣ�
        if (e.enableFade)
        {
            backgroundManager.FadeToAlpha(e.fadeToAlpha, e.fadeDuration);
        }

        // ��ת����
        if (e.enableRotation)
        {
            if (e.rotateDuration > 0)
                backgroundManager.RotateToOverTime(e.rotateToAngle, e.rotateDuration);
            else
                backgroundManager.RotateTo(e.rotateToAngle);
        }

        // ����ϵͳ�л��봵ɢ����
        if (fogSpawner != null)
        {
            if (e.toggleFog)
            {
                fogSpawner.StartSpawning();
                wasFogOn = true;
            }
            else
            {
                fogSpawner.StopSpawning();

                // ���֮ǰ������״̬ �� ����
                if (wasFogOn)
                {
                    Vector2 direction = Vector2.down;

                    fogSpawner.BlowAwayAllFog(direction, 2f);
                    wasFogOn = false;
                }
            }
        }

    }
    public void PlayAudio(AudioEvent e)
    {
        if (e.clip == null) return;

        if (e.useOneShot)
            GeneralAudioPool.Instance.PlayOneShot(e.clip, e.volume);
        else
            GeneralAudioPool.Instance.Play(e.clip, e.volume, e.loop);
    }

    public void DisplayObject(DisplayEvent e)
    {
        if (e.prefab == null) return;

        GameObject obj = Instantiate(e.prefab);
        obj.SetActive(false);

        if (e.isUI)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();

            // һ���ȹ��ϸ����壬false ��ʾ���ֱ�������/��ת
            rt.SetParent(canvas.transform, false);

            obj.SetActive(true);
        }
        else
        {
            obj.transform.position = e.position;
            obj.SetActive(true);
        }
    }

    public void DeployBoss(BossEvent e) 
    {
        BossManager.Instance.StartBoss(e.bossData);
    }

    public void SetTime(float t)
    {
        timer = t;
        eventIndex = 0;

        // ���ó�����������յ��ˡ�ֹͣ������Ƶ
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            PoolManager.Instance.Recycle(enemy);

        GeneralAudioPool.Instance.StopAll();

        // ���´�ͷִ�е� t
        while (eventIndex < allEvents.Count && allEvents[eventIndex].time <= timer)
        {
            eventIndex++;
        }

        // Ȼ������ͬ�� BGM
        SyncBGM(t);
    }
    void SyncBGM(float t)
    {
        foreach (var audioEvent in stageData.audioEvents)
        {
            float relativeTime = t - audioEvent.time;
            if (relativeTime < 0) continue; // ��û����

            if (!audioEvent.loop && relativeTime > audioEvent.clip.length)
                continue; // ��ѭ�����Ѿ�����

            // ���Ų�������ȷλ��
            AudioSource source = GeneralAudioPool.Instance.PlayAndReturnSource(audioEvent.clip, audioEvent.volume, audioEvent.loop);
            source.time = Mathf.Clamp(relativeTime, 0, audioEvent.clip.length - 0.01f);
        }
    }
}
