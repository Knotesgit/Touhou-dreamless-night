using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletMoveType
    {
        Straight,
        Sway,
        CurveTurn,
        Orbit
    }

    [Header("General")]
    public BulletMoveType moveType = BulletMoveType.Straight;
    public bool undestroyble=false;

    [Header("Sway config")]
    public float swayAmplitude = 0f;
    public float swayFrequency = 0f;
    private float swayTimer = 0f;
    private Vector2 startPosition;

    [Header("CurveTurn config")]
    public float curveRotationSpeed = 0f;
    public float curveTurnDuration = 0f;
    private float curveTurnTimer = 0f;

    [Header("Orbit config")]
    public Vector2 orbitCenter;
    public float orbitRadius = 0f;
    public float orbitSpeed = 0f;
    private float orbitAngle = 0f;

    [Header("Acceleration")]
    public bool useAcceleration = false;
    public float accelDuration = 0f;
    public float startSpeed = 0f;   // 起始速度
    private float endSpeed = 0f;
    private float currentSpeed = 0f;
    private float accelTimer = 0f;



    private Vector2 direction;
    private Vector2 initialDirection;
    private float delayTimer = 0f;
    private bool isDelayed = false;

    public GameObject disappearEffect;

    // === Initialize ===
    public void Initialize(Vector2 dir, float spd, float lifeTime, float delay = 0f)
    {
        CancelInvoke();
        StopAllCoroutines();  // 保证完全从头干净启动
        StartCoroutine(DelayEffect());

        direction = dir.normalized;
        initialDirection = direction;
        endSpeed = spd;
        startPosition = transform.position;

        orbitAngle = 0f;
        swayTimer = 0f;
        curveTurnTimer = 0f;

        delayTimer = delay;
        isDelayed = delay > 0;
        accelTimer = 0f;
        currentSpeed = 0f;

        Invoke(nameof(DestroySelf), lifeTime);
    }
    private IEnumerator DelayEffect()
    {
        yield return new WaitForSeconds(0.01f);
        PoolManager.Instance.Get(disappearEffect, transform.position);
    }

    void Update()
    {
        if (isDelayed)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isDelayed = false;
                accelTimer = 0f;
                currentSpeed = 0f;
            }
            return; // 延迟期间什么都不做
        }

        // 加速
        if (useAcceleration && accelDuration > 0f)
        {
            accelTimer += Time.deltaTime;
            float t = Mathf.Clamp01(accelTimer / accelDuration);
            currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);
        }
        else
        {
            currentSpeed = endSpeed;
        }

        // 移动
        switch (moveType)
        {
            case BulletMoveType.Straight:
                transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);
                break;

            case BulletMoveType.Sway:
                swayTimer += Time.deltaTime;
                float swayAngle = Mathf.Sin(swayTimer * swayFrequency * Mathf.PI * 2f) * swayAmplitude;
                direction = Rotate(initialDirection, swayAngle);
                transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);
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
                transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);
                transform.right = direction;
                break;

            case BulletMoveType.Orbit:
                orbitAngle += orbitSpeed * Time.deltaTime;
                float rad = orbitAngle * Mathf.Deg2Rad;
                transform.position = orbitCenter + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
                break;
        }

        if (transform.position.y > 4 || transform.position.y < -4
            || transform.position.x >= 3 || transform.position.x <= -5)
        {
            DestroySelf();
        }
    }

    public void DestroySelf()
    {
        if (undestroyble) 
        {
            return;
        }
            CancelInvoke();
        StopAllCoroutines();
        
        PoolManager.Instance.Get(disappearEffect, transform.position);
        PoolManager.Instance.Recycle(gameObject);
    }

    public void BombDestroy()
    {
        DestroySelf();
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            DestroySelf();
        }
    }

    public void SetAcceleration(bool use) => useAcceleration = use;
    public void SetAccelDuration(float dur) => accelDuration = dur;

    public void SetStartSpeed(float start) => startSpeed = start;

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}