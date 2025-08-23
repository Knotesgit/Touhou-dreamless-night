using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BulletPatternData
{
    public enum BulletFireMode
    {
        Fan,
        Stream,
        Ring,
        Custom
    }
    public enum BulletType 
    {
        Normal,
        Laser
    }
    public enum BulletMoveType
    {
        Straight,
        Sway,
        CurveTurn,
        Orbit
    }

    [Header("Basic setting")]
    public BulletType bulletType = BulletType.Normal;
    public BulletFireMode fireMode = BulletFireMode.Fan;
    public int bulletCount = 8;
    public float startAngle = 0f;
    public bool inheritRotation = false;
    public float angleBetween = 10f;
    public float bulletInterval = 0f;
    public bool inverse =false;
    public float bulletStartSpeed = 0f;
    public float bulletEndSpeed = 5f;
    public bool useAcceleration = false;
    public float accelDuration = 0f;
    public float lifeTime = 10f;
    public AudioClip bulletClip;
    public GameObject bulletPrefab;

    [Header("Delay")]
    public bool delayFireAfterSpawn = false;
    public float stopDuration = 0f;

    [Header("Aiming (Stream)")]
    public bool aimAtPlayer = false;
    public bool aimPlayerRealTime = false;

    [Header("Random Angle (Ring)")]
    public bool randomStartAngle = false;

    [Header("Rotation Offset (Ring)")]
    public bool rotating = false;
    public float rotationSpeed = 15f;
    [HideInInspector] public float currentRotationOffset = 0f;

    public enum SpawnShape { None, Circle, Spiral, Line, CustomOffsets }
    [Header("Spawn Shape")]
    public SpawnShape spawnShape = SpawnShape.None;
    public float spawnRadius = 1f;
    public bool fixedStartAngle = false;
    public Vector2[] customSpawnOffsets;

    [Header("Laser Settings")]
    public LaserSettings laser;

    [Header("Bullet Move Type")]
    public BulletMoveType moveType = BulletMoveType.Straight;

    [Header("Sway Settings")]
    public SwaySettings sway;

    [Header("CurveTurn Settings")]
    public CurveTurnSettings curveTurn;

    [Header("Orbit Settings")]
    public OrbitSettings orbit;

    [HideInInspector] public int firedCount = 0;
    [HideInInspector] public float nextFireTime = 0f;

    public List<float> customAngles;
}

[Serializable]
public struct LaserSettings
{
    public Transform followTarget;
    public bool followDirection;
    public float warnDuration;
    public float expandDuration;
    public float maxWidth;
    public float maxLength;
    public AudioClip laserClip;
    public bool synOnBulletInterval;
    public bool shakeCamera;
    public float intensity;
    public float duration;
    public bool setChild;
}

[Serializable]
public struct SwaySettings
{
    public float amplitude;
    public float frequency;
}

[Serializable]
public struct CurveTurnSettings
{
    public float rotationSpeed;
    public float duration;
}

[Serializable]
public struct OrbitSettings
{
    public float radius;
    public float speed;
}