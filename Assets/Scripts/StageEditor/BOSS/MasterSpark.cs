using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MasterSpark : MonoBehaviour
{
    public bool inverse = false;
    public Collider2D Collider;
    public SpriteRenderer sr;
    public GameObject center;
    public GameObject whiteTransition;
    public GameObject Warning;
    public Sprite highLight;
    public float lifeTime;
    [Header("Length setting")]
    public float maxLength;
    public float extendDuration;
    [Header("Warn duration")]
    public float warnDuation;
    [Header("Width setting")]
    public float maxWidth;
    public float expandDuration;
    [Header("Alpha setting")]
    public float alpha;
    public float fadeOutDuration;
    [Header("Speed setting")]
    public float startSpeed=0f;
    public float endSpeed=0f;
    public float accelDuration=0f;
    private float accelTimer = 0f;
    private float currentSpeed = 0f;


    private List<GameObject> centers = new();
    private List<SpriteRenderer> centerSprites = new();
    private SpriteRenderer centerSprite;
    private Sequence sq;
    private Vector3 worldPosition;


    void Start()
    {
        // 记录初始世界位置
        worldPosition = transform.position;
    }
    private void OnEnable()
    {
        this.DOKill();
        CancelInvoke();
        transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        sr.color = new Color(1, 1, 1, alpha);
        Collider.enabled = true;
        Invoke(nameof(DestroySelf),lifeTime);
        currentSpeed = 0f;


        for (int i = 0; i < 3; i++) 
        {
            GameObject c = GameObject.Instantiate(center, transform);
            c.transform.rotation = this.transform.rotation;
            c.transform.localScale = new Vector3(1f, (1f - (i + 1) * 0.2f), 1);
            centerSprite = c.GetComponent<SpriteRenderer>();
            centerSprite.sortingOrder = i + 1;
            centers.Add(c);
            centerSprites.Add(centerSprite);
        }
        sq = DOTween.Sequence();
        sq.AppendCallback(() => 
        {
            var w = GameObject.Instantiate(Warning);
            w.transform.position = this.transform.position;
            
        });
        sq.AppendCallback(() => GameObject.Instantiate(whiteTransition));
        sq.Append(transform.DOScaleX(maxLength, extendDuration));
        sq.AppendInterval(warnDuation);
        sq.Append(transform.DOScaleY(maxWidth, expandDuration)).OnComplete(() => 
        {
            if (centerSprites.Count != 0)
            {
                foreach(SpriteRenderer sr in centerSprites)
                {
                    sr.sprite = highLight;
                }
            }

        });
    }
    private void Update()
    {
        if (accelDuration > 0f)
        {
            accelTimer += Time.deltaTime;
            float t = Mathf.Clamp01(accelTimer / accelDuration);
            currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);
        }
        else
        {
            currentSpeed = endSpeed;
        }
        transform.Translate(transform.right * currentSpeed * Time.deltaTime, Space.World);
    }
    void LateUpdate()
    {
        if(transform.parent!=null)
            transform.position = worldPosition;
    }
    private void OnDisable()
    {
        this.DOKill();
        sq.Kill();
        centers.Clear();
        centerSprites.Clear();
        CancelInvoke();
    }
    public void DestroySelf()
    {
        this.DOKill();
        CancelInvoke();
        Collider.enabled = false;
        sr.DOFade(0f, fadeOutDuration).OnComplete(() =>
        {
            foreach (GameObject c in centers) 
            {
                c.transform.parent = null;
            }
            centers.Clear();
            centerSprites.Clear();
            PoolManager.Instance.Recycle(gameObject); 
        });
    }
    
}
