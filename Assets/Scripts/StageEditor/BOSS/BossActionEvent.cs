using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework.Internal.Commands;
using UnityEngine;

[System.Serializable]
public class BossActionEvent
{
    // ==============================
    // 配置字段（由关卡编辑器或手动设置）
    // ==============================

    // 【对白相关】
    [Header("Dialogues")]
    public bool isDialogueOnly; // 若为 true，仅播放对白，不触发符卡演出
    public List<DialogueLine> dialogueLines; // 白名单列表，支持角色头像与文本

    // 【符卡信息】
    [Header("Spell")]
    public string spellName; // 符卡名称，用于文字演出
    public float spellNamePositionX; // 文字演出时 X 坐标
    public Sprite spellSprite; // 符卡图标
    public AudioClip clearSE; // 非符卡击破 SE
    public AudioClip clearSpellSE; // 符卡击破 SE
    public AudioClip secondTickSE; // 剩余 10 秒内每秒提示音效
    public float maxHP; // 本阶段最大 HP
    public float timeLimit; // 本阶段限制时间
    public bool isTimeSpell =false;
    private float spellTimer; // 实际计时器（倒计时）
    public List<PatternSpawnInfo> patterns; // bullet pattern 列表
    public int spellBonusScore;
    public GameObject dropItem;
    private GameObject spellBG;

    // 【其他参数】
    [Header("Misc")]
    public float invincibleTime = 1.0f; // 开局无敌时间
    private float timer = 0f; // 从开始执行起的累计时间
    private int lastAlertSecond = -1; // 上一次播放秒提示音的时间戳，避免重复播放
    
    
    public bool skip = true; // debug

    // 【Boss 移动逻辑】
    [Header("Movement")]
    public List<BossMovementStep> movementSteps; // 移动指令序列
    private float currentHP; // 当前血量
    private BossHealthRing bossRing; // 环形血条引用
    private PlayerController player; // 玩家引用，用于发弹/锁定等行为
    public IEnumerator Execute(BossManager context)
    {

        BossController.Instance.InConversation = true;
        bossRing = context.currentBossTransform.GetComponentInChildren<BossHealthRing>();
        if (bossRing != null)
        {
            bossRing.gameObject.SetActive(false);
        }
        // 播放对白（若存在）
        if (dialogueLines != null && dialogueLines.Count > 0)
            yield return context.dialogueSystem.PlayDialogue(dialogueLines);

        if (isDialogueOnly)
        {
            bossRing.gameObject.SetActive(true);
            yield break;
        }
        BossController.Instance.InConversation = false;
        BossController.Instance.SetAction(this);

        if (!string.IsNullOrWhiteSpace(spellName))
        {
            yield return PlaySpellCardIntro(context, spellName, spellSprite);
        }
        
        player = context.dialogueSystem.player;
        player.SpellBonus = true;
        var movementCtrl = context.currentBossTransform.GetComponent<BossMovementController>();
        Coroutine movementRoutine = null;
        if (movementCtrl != null)
        {
            movementCtrl.playerTransform = player.transform; //传入玩家
            movementRoutine = context.StartCoroutine(movementCtrl.StartMovementSequence(movementSteps));
        }
        // 执行符卡逻辑
        currentHP = maxHP;
        timer = 0f;
        spellTimer = timeLimit;
        lastAlertSecond = -1;
        List<GameObject> spawnedPatterns = new();

        //Hp Ring
        
        if (bossRing != null)
        {
            bossRing.gameObject.SetActive(true);
            bossRing.Initialize(maxHP);
            if (isTimeSpell)
            {
                bossRing.gameObject.SetActive(false);
            }
            
        }

        Coroutine patternRoutine = context.StartCoroutine(SpawnPatterns(patterns, context.currentBossTransform, spawnedPatterns));

        while (true)
        {
            
            if (context.IsAbortRequested()) break;

            timer += Time.deltaTime;

            if (timer >= invincibleTime) 
            { 
                spellTimer -= Time.deltaTime; 
            }
            bool timeOver = spellTimer <= 0;
            bool hpDepleted = currentHP <= 0;
            
            if (context.timerText != null)
            {
                float displayTime = Mathf.Max(0f, spellTimer);
                context.timerText.text = displayTime.ToString("F1") + "s";

                if (displayTime <= 10f)
                { 
                    context.timerText.color = Color.red;
                    int wholeSecond = Mathf.FloorToInt(displayTime);
                    if (wholeSecond != lastAlertSecond)
                    {
                        lastAlertSecond = wholeSecond;
                        GeneralAudioPool.Instance.PlayOneShot(secondTickSE);
                    }
                }
                else
                    context.timerText.color = Color.white;
            }

            if (timeOver || hpDepleted)
            {
                if (context.timerText != null)
                {
                    context.timerText.text = "";
                }
                if (!string.IsNullOrWhiteSpace(spellName))   
                {
                    GameObject.Destroy(spellBG);
                    BossController.Instance.passOneSpell();
                    GeneralAudioPool.Instance.PlayOneShot(clearSpellSE);
                    if (player.SpellBonus)
                    {
                        Blink(context.spellBonus);
                        ScoreUIManager.Instance.AddScore(spellBonusScore);
                        PlayerScore.numOfCardBonusGet+=1;

                    }
                    else
                    {
                        Blink(context.spellFail);
                    }
                }
                else  
                { 
                    GeneralAudioPool.Instance.PlayOneShot(clearSE);
                }
                break;
            }
            yield return null;
        }
        



        yield return null;

        // Spell decalre anime reset
        if (context.spellNameText != null && !string.IsNullOrWhiteSpace(spellName))
        {
            context.spellNameText.DOKill();
            context.spellNameText.DOFade(0f, 0.5f);
        }

        if (context.spellSpriteRenderer != null)
        {
            context.spellSpriteRenderer.DOKill();
            context.spellSpriteRenderer.DOFade(0f, 0.5f);
        }
        
        // Handle Routines
        if (patternRoutine != null)
            context.StopCoroutine(patternRoutine); // 如果你想强制结束未发射完的 pattern
        if (movementCtrl != null)
            context.StopCoroutine(movementRoutine);
        
        // Drop item
        if(dropItem!= null)
            PoolManager.Instance.Get(dropItem, context.currentBossTransform.position);
        // Destroy patterns
        foreach (var go in spawnedPatterns)
            GameObject.Destroy(go);

        player.ClearAllEBullet();
        yield return null;
        player.ClearAllEBullet();
        bossRing.gameObject.SetActive(true);
        player.ForceCollectAllItems();
    }

    public void TakeDamage(float amount)
    {
        if (isTimeSpell)
            return;
        if (timer < invincibleTime)
            return; // 开局无敌，不吃伤害
        if (currentHP == 0) return;
        currentHP = Mathf.Max(0, currentHP - amount);
        if (bossRing != null)
        {
            bossRing.SetHP(currentHP);
        }
    }
    IEnumerator SpawnPatterns(List<PatternSpawnInfo> patterns, Transform bossTransform, List<GameObject> outList)
    {
        foreach (var p in patterns)
        {
            yield return new WaitForSeconds(p.delay);
            if (p.clearBullet)
            {
                player.ClearAllEBullet();
            }

            GameObject go = GameObject.Instantiate(p.patternPrefab);
            if (p.attachToBoss && bossTransform != null)
            {
                go.transform.SetParent(bossTransform, false); // 不保留世界坐标
                go.transform.localPosition = p.localOffset;
            }
            else
            {
                go.transform.position = p.localOffset;
            }

            outList.Add(go);
        }
    }

    private IEnumerator PlaySpellCardIntro(BossManager context, string spellName, Sprite spellSprite)
    {

        spellBG = GameObject.Instantiate(context.currentBossData.spellBG);
        // Sprite 动画（已复用）
        if (context.spellSpriteRenderer != null && spellSprite != null)
        {
            var sr = context.spellSpriteRenderer;
            sr.DOKill();
            sr.sprite = spellSprite;
            sr.color = new Color(1, 1, 1, 1);
            sr.transform.position = new Vector2(-0.68f, -0.49f);
            sr.transform.DOMove(new Vector2(-0.68f, 0.3f), 3f);
            sr.DOFade(0f, 2f);
        }

        // Spell 名称文字演出
        if (context.spellNameText != null)
        {
            var t = context.spellNameText;
            t.DOKill();
            t.text = spellName;
            t.alpha = 0f;
            t.transform.position = new Vector2(spellNamePositionX, -2.65f);
            t.DOFade(1f, 0.3f);
            t.transform.DOMove(new Vector2(spellNamePositionX, 2.68f), 1f).SetEase(Ease.OutSine).SetDelay(1.5f);
        }

        yield return new WaitForSeconds(1.2f);
    }
    void Blink(SpriteRenderer sr)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(sr.DOFade(1f, 0.1f));
        seq.AppendInterval(2f);
        seq.Append(sr.DOFade(0f, 0.1f));
    }

}