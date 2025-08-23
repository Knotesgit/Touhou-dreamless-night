
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

using Object = UnityEngine.Object;
using UnityEngine.InputSystem.Interactions;
public class PlayerController : MonoBehaviour
{
    [Header("Effect")]
    public GameObject getHittedEffect;
    [Header("Bullet prehabs")]
    public GameObject PBullet;
    public GameObject PBulletT;
    [Header("HitBox setting")]
    public HitboxVisualController hitboxL;
    public HitboxVisualController hitboxR;
    [Header("Audio setting")]
    public AudioClip shooting;
    public AudioClip getHit;
    [Header("Fire points")]
    public Transform firePointL;
    public Transform firePointR;
    public Transform firePointTLU;
    public Transform firePointTLB;
    public Transform firePointTRU;
    public Transform firePointTRB;

    [Header("Fire rate")]
    public float fireCooldown = 0.1f;
    public float fireCooldownT= 0.3f;
    [Header("Speed")]
    public float speed;
    public float slowModeSpeed;
    [Header("Player data")]
    public int lives;
    public int liveFragments;
    public int score;
    public float collectLineY = 2.5f;
    public float invincibleTime = 5f;
    public float deathDelayWindow = 0.15f;
    public BombController bombController;
    private Vector3 originPosLU;
    private Vector3 originPosLB;
    private Vector3 originPosRU;
    private Vector3 originPosRB;
    private Animator animator;
    private Vector2 position;
    private LivesUIManager livesUI;
    private BombUIManager bombUI;
    private GameOverController gameOverUI;
    private bool getHitted = false;
    private bool isInvincible = false;
    private bool isWaitingForDeath = false;
    private bool inConversation = false;
    private bool spellBonus = false;
    public bool SpellBonus
    {
        get => spellBonus;
        set => spellBonus = value;
    }
    public bool InConversation
    {
        set => inConversation = value;
    }
    private Coroutine deathCoroutine;
    private Coroutine invincibilityRoutine;
    
    void Start()
    {
        PlayerScore.reset();
        animator = GetComponent<Animator>();
        originPosLU = firePointTLU.localPosition;
        originPosLB = firePointTLB.localPosition;
        originPosRU = firePointTRU.localPosition;
        originPosRB = firePointTRB.localPosition;
        livesUI = GameObject.Find("UILivesManager").GetComponent<LivesUIManager>();
        bombUI = GameObject.Find("UIBombsManager").GetComponent<BombUIManager>();
        gameOverUI = GameObject.Find("GameOverController").GetComponent<GameOverController>();
        lives = livesUI.GetLifeCount();
        liveFragments = livesUI.GetFragmentCount();
    }

    // Update is called once per frame
    void Update()
    {

        float currentSpeed = speed;
        float horizontal = 0f;
        float vertical = 0f;
        hitboxL.Hide();
        hitboxR.Hide();

        if (transform.position.y >= collectLineY)
        {
            ForceCollectAllItems();
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            horizontal = -1f;
            
        }
        else if(Keyboard.current.rightArrowKey.isPressed) 
        {
            horizontal = 1f;
        }
        if (Keyboard.current.upArrowKey.isPressed)
        {
            vertical = 1f;
        }
        else if(Keyboard.current.downArrowKey.isPressed)
        {
            vertical= -1f;
        }
        if (Keyboard.current.leftShiftKey.isPressed) 
        {
            hitboxL.Show();
            hitboxR.Show();
            currentSpeed = slowModeSpeed;
            firePointTLU.DOLocalMove(new Vector2(-0.25f,0.21f),0.2f);
            firePointTLB.DOLocalMove(new Vector2(-0.1f, 0.35f), 0.2f);
            firePointTRU.DOLocalMove(new Vector2(0.25f, 0.21f), 0.2f);
            firePointTRB.DOLocalMove(new Vector2(0.1f, 0.35f), 0.2f);
        }
        else
        {
            firePointTLU.DOLocalMove(originPosLU, 0.2f);
            firePointTLB.DOLocalMove(originPosLB, 0.2f);
            firePointTRU.DOLocalMove(originPosRU, 0.2f);
            firePointTRB.DOLocalMove(originPosRB, 0.2f);
        }
        if (!inConversation) 
        {
            if (Keyboard.current.xKey.isPressed && !getHitted)
            {
                bombController.TriggerBomb();
            }
            if (Keyboard.current.zKey.isPressed)
            {
                shoot();
            }
            
            
        }
        if (Time.timeScale != 0f)
        {
            animator.SetFloat("Horizontal", horizontal);
        }
        position.x = horizontal * currentSpeed * Time.deltaTime + transform.position.x;
        position.y = vertical * currentSpeed * Time.deltaTime + transform.position.y;
        if (!getHitted)
        {
            position.x = Mathf.Clamp(position.x, -3.5f, 1.1f);
            position.y = Mathf.Clamp(position.y, -2.65f, 2.7f);
        }
        transform.position = position;


        lives = livesUI.GetLifeCount(); 
        liveFragments = livesUI.GetFragmentCount();
        if (lives < 0)
        {
            gameOver();
        }
    }

    
    float fireTimer = 0f;
    float fireTimerT= 0f;
    void shoot()
    {
        fireTimer -= Time.deltaTime;
        fireTimerT -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            PoolManager.Instance.Get(PBullet, firePointL.position);
            PoolManager.Instance.Get(PBullet, firePointR.position);
            
            fireTimer = fireCooldown;
            
        }
        if (fireTimerT <= 0) 
        {
            PoolManager.Instance.Get(PBulletT, firePointTLU.position);
            PoolManager.Instance.Get(PBulletT, firePointTRU.position);
            PoolManager.Instance.Get(PBulletT, firePointTLB.position);
            PoolManager.Instance.Get(PBulletT, firePointTRB.position);
            fireTimerT = fireCooldownT;
            GeneralAudioPool.Instance.PlayOneShot(shooting, 0.1f);
        }
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        CollectibleItem collectibleItem = collision.GetComponent<CollectibleItem>();
        if (collectibleItem != null)
        {
           if (!collectibleItem.collected)
           {
                collectibleItem.collected = true;
                switch (collectibleItem.collectableType)
                {
                    case CollectibleItem.CollectableType.heart:
                        livesUI.AddLife();
                        break;
                    case CollectibleItem.CollectableType.heartFrag:
                        livesUI.AddFragment();
                        break;
                    case CollectibleItem.CollectableType.star:
                        bombUI.AddBomb();
                        break;
                    case CollectibleItem.CollectableType.score:
                        ScoreUIManager.Instance.AddScore(100);
                        break;
                }
            }
            
        }
        if(collision.CompareTag("EBullet"))
        {
            if (!isInvincible && !isWaitingForDeath) 
            {
                isWaitingForDeath = true;
                deathCoroutine = StartCoroutine(DeathDelayCoroutine());
            }
            //PoolManager.Instance.Get(getHittedEffect, transform.position);
            //GeneralAudioPool.Instance.PlayOneShot(getHit);
        }
    }
    public IEnumerator DeathDelayCoroutine()
    {
        if (isInvincible)
            yield break;
        float timer = 0f;

        // 播放击中特效和音效（先不扣命）
        
        GeneralAudioPool.Instance.PlayOneShot(getHit);
        while (timer < deathDelayWindow)
        {
            if (Input.GetKeyDown(KeyCode.X) && bombUI.GetBombCount() > 0)
            {
                // 决死成功
                bombController.TriggerBomb();
                getHitted = false;
                isWaitingForDeath = false;
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 决死失败，正式受伤
        //lives--;
        //  Debug start
        //lives++;
        //  Debug end
        Instantiate(getHittedEffect, transform.position, Quaternion.identity);
        getHitted = true;
        transform.position = new Vector2(-1.218f, -4f);
        ClearEBullet();
        livesUI.ReduceLife();
        if (bombUI.GetBombCount()< 2) 
        {
            bombUI.SetBombCount(2);
        }
        transform.DOMove(new Vector2(-1.218f, -2.219f), 1f).OnComplete(() => getHitted = false);
        StartInvincibility(invincibleTime);
        isWaitingForDeath = false;
        PlayerScore.numOfMiss += 1;
    }

    public void StartInvincibility(float duration)
    {
        if (invincibilityRoutine != null) { StopCoroutine(invincibilityRoutine); }
        invincibilityRoutine = StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float timer = 0f;

        while (timer < duration)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.1f);
            sr.enabled = true;
            yield return new WaitForSeconds(0.1f);
            timer += 0.2f;
        }

        isInvincible = false;
    }

    void gameOver() 
    {
        
        Time.timeScale = 0f;
        gameOverUI.ShowGameOverMenu();
        this.enabled = false;
    }
    public void ForceCollectAllItems()
    {
        CollectibleItem[] items = Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        foreach (var item in items)
        {
            item.ForceCollect();
        }
    }
    public void ClearEBullet() 
    {
        spellBonus = false;
        Bullet[] bullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (var bullet in bullets)
        {
            bullet.BombDestroy();
        }

    }
    public void ClearAllEBullet()
    {
        Bullet[] bullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        Laser[] lasers = Object.FindObjectsByType<Laser>(FindObjectsSortMode.None);
        foreach (var bullet in bullets)
        {
            bullet.BombDestroy();
        }
        foreach (var laser in lasers) 
        {
            laser.DestroySelf();
        }

    }

}
