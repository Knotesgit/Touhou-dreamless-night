using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour,IHittable
{
    public static BossController Instance;
    public event Action OnBossDefeated;
    public int spellCount;
    public GameObject spellPassEffect;
    private bool inConversation = true;
    private BossActionEvent currentAction;
    public bool InConversation
    {
        get { return inConversation; }
        set { inConversation = value; }
    }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpellStarCounter.Instance.Initialize(spellCount);
    }
    private void Update()
    {
        if(!inConversation)
            ScoreUIManager.Instance.AddScore(1);
        if (spellCount == -1) 
        {
            destroyRoutine();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            StartCoroutine(player.DeathDelayCoroutine());
        }
        

    }

    public void SetAction(BossActionEvent action)
    {
        currentAction = action;
    }

    public void OnHit(float damage)
    {
        ScoreUIManager.Instance.AddScore(1);
        if (currentAction != null)
        {
            currentAction.TakeDamage(damage);
        }
    }

    public void passOneSpell() 
    {
        spellCount--;
        SpellStarCounter.Instance.UseOneSpell();
        if (!currentAction.isTimeSpell)
            GameObject.Instantiate(spellPassEffect,this.gameObject.transform.position,transform.rotation);
    }
    private void destroyRoutine() 
    {

        InConversation = true;
        PlayerScore.score = ScoreUIManager.Instance.GetScore();
        OnBossDefeated?.Invoke();
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
