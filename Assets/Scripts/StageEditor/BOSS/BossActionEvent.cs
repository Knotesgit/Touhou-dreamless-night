using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework.Internal.Commands;
using UnityEngine;

[System.Serializable]
public class BossActionEvent
{
    // ==============================
    // �����ֶΣ��ɹؿ��༭�����ֶ����ã�
    // ==============================

    // ���԰���ء�
    [Header("Dialogues")]
    public bool isDialogueOnly; // ��Ϊ true�������Ŷ԰ף������������ݳ�
    public List<DialogueLine> dialogueLines; // �������б�֧�ֽ�ɫͷ�����ı�

    // ��������Ϣ��
    [Header("Spell")]
    public string spellName; // �������ƣ����������ݳ�
    public float spellNamePositionX; // �����ݳ�ʱ X ����
    public Sprite spellSprite; // ����ͼ��
    public AudioClip clearSE; // �Ƿ������� SE
    public AudioClip clearSpellSE; // �������� SE
    public AudioClip secondTickSE; // ʣ�� 10 ����ÿ����ʾ��Ч
    public float maxHP; // ���׶���� HP
    public float timeLimit; // ���׶�����ʱ��
    public bool isTimeSpell =false;
    private float spellTimer; // ʵ�ʼ�ʱ��������ʱ��
    public List<PatternSpawnInfo> patterns; // bullet pattern �б�
    public int spellBonusScore;
    public GameObject dropItem;
    private GameObject spellBG;

    // ������������
    [Header("Misc")]
    public float invincibleTime = 1.0f; // �����޵�ʱ��
    private float timer = 0f; // �ӿ�ʼִ������ۼ�ʱ��
    private int lastAlertSecond = -1; // ��һ�β�������ʾ����ʱ����������ظ�����
    
    
    public bool skip = true; // debug

    // ��Boss �ƶ��߼���
    [Header("Movement")]
    public List<BossMovementStep> movementSteps; // �ƶ�ָ������
    private float currentHP; // ��ǰѪ��
    private BossHealthRing bossRing; // ����Ѫ������
    private PlayerController player; // ������ã����ڷ���/��������Ϊ
    public IEnumerator Execute(BossManager context)
    {

        BossController.Instance.InConversation = true;
        bossRing = context.currentBossTransform.GetComponentInChildren<BossHealthRing>();
        if (bossRing != null)
        {
            bossRing.gameObject.SetActive(false);
        }
        // ���Ŷ԰ף������ڣ�
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
            movementCtrl.playerTransform = player.transform; //�������
            movementRoutine = context.StartCoroutine(movementCtrl.StartMovementSequence(movementSteps));
        }
        // ִ�з����߼�
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
            context.StopCoroutine(patternRoutine); // �������ǿ�ƽ���δ������� pattern
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
            return; // �����޵У������˺�
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
                go.transform.SetParent(bossTransform, false); // ��������������
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
        // Sprite �������Ѹ��ã�
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

        // Spell ���������ݳ�
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