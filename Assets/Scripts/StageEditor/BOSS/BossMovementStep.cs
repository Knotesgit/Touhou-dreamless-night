using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossMovementStep
{
    public enum BossMoveType
    {
        Stay,
        ToFixedPoint,
        ChasePlayer,
        SmartChaseRandom  //伪随机 + 朝向玩家 + 停顿冷却
    }

    public BossMoveType moveType = BossMoveType.Stay;
    public Vector2 targetPoint = new Vector2(-1.26f, 2.12f);      // For ToFixedPoint
    
    [Header("Step Duration")]
    public float duration = 2f;

    [Header("smart chase setting")]
    public float maxMoveRange;
    public Vector2 randomAreaSize = new Vector2(3f, 2f); // For RandomWithinArea
    public float coolDown=1f;

    [Header("Speed(and acceleration) setting")]
    public float startSpeed = 0f;
    public float targetSpeed = 3f;
    public float accelTime = 0.5f; // 多久加到 targetSpeed
}
