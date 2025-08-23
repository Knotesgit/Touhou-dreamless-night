using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Laser : MonoBehaviour
{
    public enum BulletMoveType
    {
        Straight,
        Sway,
        CurveTurn,
        Orbit
    }
    [Header("Basic Settings")]
    public Collider2D laserCollider;
    public SpriteRenderer spriteRenderer;
    public float fadeDuration = 0.5f;

    [Header("Follow")]
    public Transform followTransform;    // 持续跟随旋转 / 位置的发射体
    public bool followDirection = false;

    [Header("General")]
    public BulletMoveType moveType = BulletMoveType.Straight;

    [Header("Sway config")]
    public float swayAmplitude = 0f;
    public float swayFrequency = 0f;
    private float swayTimer = 0f;
    private Vector2 startPosition;

    [Header("CurveTurn config")]
    public float curveRotationSpeed = 0f;
    public float curveTurnDuration = 0f;
    private float curveTurnTimer = 0f;

    private float warnDuration;
    private float expandDuration;
    private float maxWidth;
    private float maxLength;
    private float lifeTime;
    private AudioClip clip;

    private Tween currentTween;
    private float delayTimer = 0f;
    private bool isDelayed = false;
    private Vector2 direction;
    private Vector2 initialDirection;
    private void OnEnable()
    {
        ResetLaser();
    }


    public void InitLaser(Vector2 dir,float warnDuration, float expandDuration, 
        float maxWidth,float maxLength,float lifeTime,
        AudioClip clip,float delay = 0f)
    {
        CancelInvoke();
        StopAllCoroutines();  // 保证完全从头干净启动

        if (followTransform != null) 
        {
            transform.position = followTransform.position;
        }
        direction = dir.normalized;
        initialDirection = direction;
        this.warnDuration = warnDuration;
        this.expandDuration = expandDuration;
        this.maxWidth = maxWidth;
        this.maxLength = maxLength;
        this.lifeTime = lifeTime;
        this.clip = clip;

        isDelayed = delay > 0f;
        delayTimer = delay;

        curveTurnTimer = 0f;
        swayTimer = 0f;
        startPosition = transform.position;

        Invoke(nameof(DestroySelf), lifeTime);

        ResetLaser();
        spriteRenderer.DOFade(1f, fadeDuration);
        // 预警期间细线
        currentTween = DOVirtual.DelayedCall(this.warnDuration, () =>
        {

            currentTween = transform.DOScaleY(this.maxWidth, expandDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    laserCollider.enabled = true;
                    if (clip != null)
                        GeneralAudioPool.Instance.PlayOneShot(clip, 1f);
                });
        });
    }

    private void ResetLaser()
    {
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();
        laserCollider.enabled = false;
        transform.localScale = new Vector3(this.maxLength, 0.1f, 1f);
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        if (isDelayed)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isDelayed = false;
            }
            return; // 延迟期间什么都不做
        }
        if (followTransform != null)
        {
            transform.position = followTransform.position;
            if (followDirection) 
            {
                transform.right = followTransform.right;
            }
            
        }

        switch (moveType)
        {
            case BulletMoveType.Straight:
                break;

            case BulletMoveType.Sway:
                swayTimer += Time.deltaTime;
                float swayAngle = Mathf.Sin(swayTimer * swayFrequency * Mathf.PI * 2f) * swayAmplitude;
                direction = Rotate(initialDirection, swayAngle);
                transform.right = direction;
                break;

            case BulletMoveType.CurveTurn:
                if (curveTurnDuration <= 0)
                {
                    direction = Rotate(direction, curveRotationSpeed * Time.deltaTime);
                }
                else if (curveTurnTimer < curveTurnDuration)
                {
                    direction = Rotate(direction, curveRotationSpeed * Time.deltaTime);
                    curveTurnTimer += Time.deltaTime;
                }
                transform.right = direction;
                break;
        }
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
    public void DestroySelf()
    {
        CancelInvoke();
        StopAllCoroutines();
        laserCollider.enabled = false;
        spriteRenderer.DOFade(0f, fadeDuration).OnComplete(()=> PoolManager.Instance.Recycle(gameObject));
    }


}
