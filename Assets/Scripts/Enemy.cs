using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour,IHittable
{
    [Header("Property")]
    public float hp = 5;
    public AudioClip clip;
    public GameObject deathEffect;
    public GameObject dropItem;

    [Header("Movement")]
    public Vector2 targetSpeed = new Vector2(0f, -2f); // x=水平, y=垂直速度
    public bool useAcceleration = false;
    public float accelDuration = 1f;
    public float stayDuration = 0f;
    private bool enableMovement = false;
    

    [Header("Floating")]
    public bool enableFloating = true;
    public float floatAmplitude = 0.1f; // 漂浮高度
    public float floatFrequency = 2f;   // 频率（越高越快）

    private float floatOffsetY; // 正弦偏移
    private float floatTimer;
    private Vector3 lastFloatOffset; // 上一帧的偏移量

    private Vector2 currentSpeed;
    private float accelTimer;

    private AudioSource audioSource;
    private BulletPattern pattern;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isDead = false;

    public static List<Enemy> ActiveEnemies = new();

    void Start()
    {
        
        audioSource = GetComponent<AudioSource>();
        pattern = GetComponent<BulletPattern>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        //currentSpeed = useAcceleration ? Vector2.zero : targetSpeed;
        //accelTimer = 0f;
        

    }
    void OnEnable()
    {
        isDead = false;
        lastFloatOffset = Vector3.zero;
        if (!ActiveEnemies.Contains(this))
            ActiveEnemies.Add(this);
    }

    void OnDisable()
    {
       CleanBulletPatterns();
       ActiveEnemies.Remove(this);
    }
    void Update()
    {
        ScoreUIManager.Instance.AddScore(1);
        // 移动逻辑
        if (enableMovement)
        {
            // 离场销毁
            if (transform.position.y >= 3 || transform.position.y <= -3
            || transform.position.x >= 1.6 || transform.position.x <= -4)
            {
                PoolManager.Instance.Recycle(gameObject);
            }
            if (useAcceleration && accelDuration > 0f)
            {
                accelTimer += Time.deltaTime;
                float t = Mathf.Clamp01(accelTimer / accelDuration);
                currentSpeed = Vector2.Lerp(Vector2.zero, targetSpeed, t);
            }
            transform.Translate(currentSpeed * Time.deltaTime, Space.World);
        }
        
        // Floating
        if (enableFloating)
        {
            floatTimer += Time.deltaTime;
            floatOffsetY = Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude;

            // 去除上一帧的偏移量，再加上当前帧的偏移量（避免叠加）
            transform.position -= lastFloatOffset;
            Vector3 offset = new Vector3(0f, floatOffsetY, 0f);
            transform.position += offset;
            lastFloatOffset = offset;
        }

    }

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        ScoreUIManager.Instance.AddScore(10);
        if (hp <= 0 && !isDead)
        {
            isDead = true;
            playSound(clip);
            die();
        }
    }
    public void OnHit(float damage) 
    {
        TakeDamage(damage);
    }

    void playSound(AudioClip audioClip)
    {
        audioSource.volume = 0.2f;
        audioSource.PlayOneShot(audioClip);
    }

    void die()
    {
        ScoreUIManager.Instance.AddScore(100);
        if (pattern != null)
        {
            pattern.StopAllCoroutines();
            pattern.enabled = false;
        }
        sr.enabled = false;
        col.enabled = false;

        PoolManager.Instance.Get(deathEffect, transform.position);
        if (dropItem != null) 
        {
            PoolManager.Instance.Get(dropItem, transform.position);
        }
        StartCoroutine(DelayedRecycle(0.2f));
    }

    IEnumerator DelayedRecycle(float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolManager.Instance.Recycle(gameObject);
    }
    public void CleanBulletPatterns()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<BulletPattern>() != null)
            {
                Destroy(child.gameObject); // 如果用对象池则 Recycle
            }
        }
    }
    public void SetMovementEnabled(bool enabled)
    {
        StartCoroutine(StayStill(enabled));
    }
    IEnumerator StayStill(bool enabled) 
    {
        yield return new WaitForSeconds(stayDuration);
        enableMovement = enabled;
    }
    public void SetTargetSpeed(Vector2 newSpeed)
    {
        targetSpeed = newSpeed;
        if (!useAcceleration)
            currentSpeed = newSpeed;
        else
            accelTimer = 0f; // 重置加速度计时器以重新加速
    }
    public bool IsMoving => enableMovement;

    public void SetAcceleration(bool use) 
    { 
        useAcceleration = use;
    }
    public bool IsAccelerating => useAcceleration;

    public void SetAccDuration(float duration) 
    { 
        accelDuration = duration;
    }

    public void SetHp(float health) 
    {
        hp = health;
    }

    public void SetDropItem(GameObject collectibleItem) 
    {
        dropItem = collectibleItem;
    }

    public void SetStayDuration(float duration) 
    {
        stayDuration = duration; 
    }
}