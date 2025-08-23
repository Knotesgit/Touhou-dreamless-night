using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }
    
    public DialogueSystem dialogueSystem;
    public Transform currentBossTransform;
    
    public SpriteRenderer spellSpriteRenderer;
    public TextMeshPro spellNameText;
    public TextMeshPro timerText;

    public SpriteRenderer spellBonus;
    public SpriteRenderer spellFail;
    
    private Coroutine bossRoutine;
    private bool isRunning = false;
    private bool abortRequested = false;

    public BossData currentBossData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetSorting(spellNameText, "EBullet", 1000);
        SetSorting(timerText, "EBullet", 1000);
        spellBonus.color = new Color(1, 1, 1, 0);
        spellFail.color = new Color(1, 1, 1, 0);

    }

    /// <summary>
    /// 启动 Boss 行为序列
    /// </summary>
    public void StartBoss(BossData data)
    {
        if (isRunning)
        {
            AbortBoss();
        }

        currentBossData = data;
        abortRequested = false;
        bossRoutine = StartCoroutine(RunBossSequence(data));
    }

    /// <summary>
    /// 主执行流程：顺序执行每个 BossEvent，直到全部完成或被中止
    /// </summary>
    private IEnumerator RunBossSequence(BossData data)
    {
        isRunning = true;

        GameObject boss = GameObject.Instantiate(data.bossPrefab, data.spawnPosition, Quaternion.identity);
        currentBossTransform = boss.transform;

        // Step 2: Entry animation using DOTween
        currentBossTransform.position = data.spawnPosition;
        Tween moveTween = currentBossTransform
            .DOMove(data.entryTargetPosition, data.entryDuration)
            .SetEase(Ease.OutSine);

        foreach (var e in data.events)
        {
            if (abortRequested)
            {
                break;
            }

            if (e == null)
            {
                continue;
            }

            yield return e.Execute(this);
        }

        isRunning = false;
    }

    /// <summary>
    /// 中止当前 boss（立即终止执行流程）
    /// </summary>
    public void AbortBoss()
    {
        if (!isRunning) return;

        abortRequested = true;

        if (bossRoutine != null)
        {
            StopCoroutine(bossRoutine);
            bossRoutine = null;
        }
        isRunning = false;
    }

    public bool IsAbortRequested()
    {
        return abortRequested;
    }
    void SetSorting(TextMeshPro tmp, string layer, int order)
    {
        if (tmp == null) return;
        var renderer = tmp.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = order;
        }
    }
}


