using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPattern : MonoBehaviour
{
    public List<TimedBulletPattern> timeline;
    private float elapsed = 0;

    void Start()
    {
        foreach (var timed in timeline)
        {
            timed.pattern.firedCount = 0;
            timed.pattern.nextFireTime = timed.timeOffset;
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        foreach (var timed in timeline)
        {
            //originally if
            while (elapsed >= timed.pattern.nextFireTime &&
                (timed.repeatCount == -1 || timed.pattern.firedCount < timed.repeatCount))
            {
                FirePattern(timed.pattern);
                timed.pattern.firedCount++;
                timed.pattern.nextFireTime += timed.repeatInterval > 0 ? timed.repeatInterval : float.MaxValue;
            }
        }
    }

    void FirePattern(BulletPatternData data)
    {
        switch (data.fireMode)
        {
            case BulletPatternData.BulletFireMode.Fan:
                StartCoroutine(FireFan(data));
                break;
            case BulletPatternData.BulletFireMode.Stream:
                StartCoroutine(FireStream(data));
                break;
            case BulletPatternData.BulletFireMode.Ring:
                FireRing(data);
                break;
            case BulletPatternData.BulletFireMode.Custom:
                FireCustom(data);
                break;
        }
    }

    IEnumerator FireFan(BulletPatternData data)
    {
        float baseAngle = data.randomStartAngle ? Random.Range(0f, 360f) : data.startAngle;
        if (data.aimAtPlayer)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector2 toPlayer = (player.transform.position - transform.position).normalized;
                baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            }
        }
        if (data.bulletInterval == 0) 
        {
            if (data.bulletType == BulletPatternData.BulletType.Laser)
            {
                GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 1f);
            }
            else
            {
                GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 0.1f);
            }
        }
        

        for (int i = 0; i < data.bulletCount; i++)
        {
            if (data.bulletInterval != 0) 
            {
                if (data.bulletType == BulletPatternData.BulletType.Laser)
                {
                    GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 1f);
                }
                else
                {
                    GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 0.1f);
                }
            }
            float angle = baseAngle + i * data.angleBetween;
            FireOneBullet(data, angle, i);
            yield return new WaitForSeconds(data.bulletInterval);
        }
    }

    IEnumerator FireStream(BulletPatternData data)
    {
        
        float angle = data.startAngle;

        if (data.aimAtPlayer)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector2 toPlayer = (player.transform.position - transform.position).normalized;
                angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            }
        }
        for (int i = 0; i < data.bulletCount; i++)
        {
            if (!data.aimPlayerRealTime)
            {
                angle += data.angleBetween;
            }
            if (data.bulletType == BulletPatternData.BulletType.Laser)
            {
                GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 1f);
            }
            else 
            {
                GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 0.1f);
            }
            
            FireOneBullet(data, angle, i);
            yield return new WaitForSeconds(data.bulletInterval);
        }
    }

    void FireRing(BulletPatternData data)
    {
        float angle = data.randomStartAngle ? Random.Range(0f, 360f): data.startAngle + data.currentRotationOffset;
        float delta = 360f / data.bulletCount;
        if (data.bulletType == BulletPatternData.BulletType.Laser)
        {
            GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 1f);
        }
        else
        {
            GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 0.1f);
        }
        for (int i = 0; i < data.bulletCount; i++)
        {
            FireOneBullet(data, angle, i);
            angle += delta;
        }

        if (data.rotating)
            data.currentRotationOffset += data.rotationSpeed;
    }

    void FireCustom(BulletPatternData data)
    {
        if (data.customAngles == null) return;
        foreach (var angle in data.customAngles)
        {
            FireOneBullet(data, angle);
        }
    }

    void FireOneBullet(BulletPatternData data, float angle, int index = 0)
    {
        if (data.bulletType == BulletPatternData.BulletType.Laser)
        {
            FireLaser(data, angle,index);
        }
        else
        {
            FireNormalBullet(data, angle, index);
        }
    }
    void FireLaser(BulletPatternData data, float angle, int index = 0)
    {
        //GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 1f);
        float warnOffset;
        float lifetimeOffset;
        if (data.laser.synOnBulletInterval)
        {
            warnOffset = data.laser.warnDuration - index * data.bulletInterval;
            lifetimeOffset = data.lifeTime - index * data.bulletInterval;
        }
        else 
        {
            warnOffset= data.laser.warnDuration;
            lifetimeOffset= data.lifeTime;
        }
        if (warnOffset < 0||lifetimeOffset<0) 
        {
            Debug.Log("warn duration: "+warnOffset+" cannot be smaller than 0");
            Debug.Log("lifetime: " + lifetimeOffset + " cannot be smaller than 0");
        }
        float rad = angle * Mathf.Deg2Rad;
        Vector2 localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        Vector2 dir = data.inheritRotation ? transform.TransformDirection(localDir).normalized : localDir;

        Vector2 offset = CalculateSpawnOffset(data, index, angle);
        if (data.inheritRotation)
        {
            offset = transform.TransformDirection(offset);
        }
        Vector2 spawnPos = (Vector2)transform.position + offset;

        GameObject laserGO = PoolManager.Instance.Get(data.bulletPrefab, spawnPos);
        laserGO.transform.right = dir;
        if (data.inverse)
        {
            laserGO.transform.right = -dir;
        }
        float delay = data.delayFireAfterSpawn ? data.stopDuration : 0f;
        Laser laser = laserGO.GetComponent<Laser>();
        if(laser!= null)
        {
            laser.moveType = (Laser.BulletMoveType)data.moveType;
            laser.followTransform = data.laser.followTarget;
            laser.swayAmplitude = data.sway.amplitude;
            laser.swayFrequency = data.sway.frequency;
            laser.curveRotationSpeed = data.curveTurn.rotationSpeed;
            laser.curveTurnDuration = data.curveTurn.duration;

            laser.InitLaser(dir,
                warnOffset,
                data.laser.expandDuration,
                data.laser.maxWidth,
                data.laser.maxLength,
                lifetimeOffset,
                data.laser.laserClip
                ,delay);
        }
        
        if (data.laser.shakeCamera) 
        {
            CameraShaker.ShakeOnce(data.laser.intensity, data.laser.duration);
        }
        if (data.laser.setChild) 
        {
            laserGO.transform.parent = this.transform;
        }
    }
    void FireNormalBullet(BulletPatternData data, float angle, int index=0)
    {
        // 播放音效
        //GeneralAudioPool.Instance.PlayOneShot(data.bulletClip, 0.1f);


        // 计算偏移位置
        Vector2 offset = CalculateSpawnOffset(data, index, angle);
        if (data.inheritRotation)
        {
            offset = transform.TransformDirection(offset);
        }
        Vector2 spawnPos = (Vector2)transform.position + offset;

        GameObject bullet = PoolManager.Instance.Get(data.bulletPrefab, spawnPos);

        float rad = angle * Mathf.Deg2Rad;
        Vector2 localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        Vector2 dir = data.inheritRotation ? transform.TransformDirection(localDir).normalized : localDir;
        //Debug.Log($"Angle: {angle}, LocalDir: {localDir}, Dir: {dir}, Magnitude: {dir.magnitude}");
        if (data.aimPlayerRealTime)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector2 toPlayer = ((Vector2)player.transform.position - spawnPos).normalized;
                dir = toPlayer;
            }
        }
        
        bullet.transform.right = dir;
        if (data.inverse) 
        {
            bullet.transform.right = -dir;
        }
        if (Mathf.Abs(Mathf.Abs(angle) - 180f) < 0.001f)
        {
            // 强制使用 Z 轴 180° 旋转
            bullet.transform.rotation = Quaternion.Euler(0, 0, 180f);
        }

        float delay = data.delayFireAfterSpawn ? data.stopDuration : 0f;
        Bullet b = bullet.GetComponent<Bullet>();
        if(b != null)
        {
            b.moveType = (Bullet.BulletMoveType)data.moveType;
            // 新增传入 struct 参数
            b.swayAmplitude = data.sway.amplitude;
            b.swayFrequency = data.sway.frequency;
            b.curveRotationSpeed = data.curveTurn.rotationSpeed;
            b.curveTurnDuration = data.curveTurn.duration; 
            b.orbitRadius = data.orbit.radius;
            b.orbitSpeed = data.orbit.speed;
            b.orbitCenter = spawnPos;

            b.SetStartSpeed(data.bulletStartSpeed);
            b.SetAccelDuration(data.accelDuration);
            b.SetAcceleration(data.useAcceleration);
            b.Initialize(dir, data.bulletEndSpeed, data.lifeTime, delay);
        }
    }

    Vector2 CalculateSpawnOffset(BulletPatternData data, int index, float angle)
    {
        if (data.fixedStartAngle)
        {
            angle = data.startAngle;
        }
        float offsetAngle = angle;
        switch (data.spawnShape)
        {
            case BulletPatternData.SpawnShape.Circle:
                {
                    float rad = angle * Mathf.Deg2Rad;
                    return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * data.spawnRadius;
                }
            case BulletPatternData.SpawnShape.Spiral:
                {
                    float rad = angle * Mathf.Deg2Rad;
                    return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (data.spawnRadius + index * 0.1f);
                }
            case BulletPatternData.SpawnShape.Line:
                {
                    //float rad = (data.startAngle+data.currentRotationOffset) * Mathf.Deg2Rad;
                    float rad = (angle+ data.currentRotationOffset) * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
                    return dir * index * data.spawnRadius;
                }
            case BulletPatternData.SpawnShape.CustomOffsets:
                {
                    if (data.customSpawnOffsets != null && index < data.customSpawnOffsets.Length)
                        return data.customSpawnOffsets[index];
                    else
                        return Vector2.zero;
                }
            default:
                return Vector2.zero;
        }
    }
}