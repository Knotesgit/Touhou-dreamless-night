using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� boss ���ƶ���Ϊ���У�֧�ֶ����ƶ���ʽ����ֹ�����㡢׷�١�α����ƶ�
/// </summary>
public class BossMovementController : MonoBehaviour
{
    public Transform playerTransform;

    public IEnumerator StartMovementSequence(List<BossMovementStep> steps)
    {
        foreach (var step in steps)
        {
            yield return ExecuteStep(step);
        }
    }

    private IEnumerator ExecuteStep(BossMovementStep step)
    {
        switch (step.moveType)
        {
            case BossMovementStep.BossMoveType.Stay:
                yield return new WaitForSeconds(step.duration);
                break;

            case BossMovementStep.BossMoveType.ToFixedPoint:
                yield return MoveToFixedPoint(step);
                break;

            case BossMovementStep.BossMoveType.ChasePlayer:
                yield return ChasePlayerOnce(step);
                break;

            case BossMovementStep.BossMoveType.SmartChaseRandom:
                yield return SmartChaseRandom(step);
                break;
        }
    }

    private IEnumerator MoveToFixedPoint(BossMovementStep step)
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = step.targetPoint;
        float elapsed = 0f;
        float speed = step.startSpeed;
        float accel = (step.accelTime > 0f) ? (step.targetSpeed - step.startSpeed) / step.accelTime : 0f;

        while (elapsed < step.duration)
        {
            float dt = Time.deltaTime;
            elapsed += Time.deltaTime;

            if (elapsed < step.accelTime)
                speed += accel * dt;
            else
                speed = step.targetSpeed;

            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            transform.Translate(dir * speed * dt);

            // ����Ŀ��㣬ͣ��
            if (Vector2.Distance(transform.position, targetPos) < 0.05f)
                break;

            yield return null;
        }

        // ͣ��ʣ��ʱ��
        float remaining = step.duration - elapsed;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);
    }

    private IEnumerator ChasePlayerOnce(BossMovementStep step)
    {
        if (playerTransform == null)
            yield break;

        float speed = step.startSpeed;
        float accel = (step.accelTime > 0f) ? (step.targetSpeed - step.startSpeed) / step.accelTime : 0f;
        float elapsed = 0f;
        bool hasReentered = false;

        Vector2 dir = (playerTransform.position - transform.position).normalized;

        while (elapsed < step.duration)
        {
            float dt = Time.deltaTime;
            elapsed += Time.deltaTime;

            if (elapsed < step.accelTime)
                speed += accel * dt;
            else
                speed = step.targetSpeed;

            transform.Translate(dir * speed * dt);

            if (!hasReentered || IsOutOfScreen(transform.position))
            {
                // �����·���
                dir = (playerTransform.position - transform.position).normalized;
                hasReentered = true;
            }

            yield return null;
        }
    }
    
    private bool IsOutOfScreen(Vector3 worldPos)
    {
        return (transform.position.y >= 5 || transform.position.y <= -5
        || transform.position.x >= 4 || transform.position.x <= -6);
    }

        private IEnumerator SmartChaseRandom(BossMovementStep step)
        {
            if (playerTransform == null)
                yield break;
            float coolDownTimer = step.coolDown;
            float elapsed = 0f;
            float perTargetElapsed = 0f;
            float speed = step.startSpeed;
            float accel = (step.accelTime > 0f) ? (step.targetSpeed - step.startSpeed) / step.accelTime : 0f;

            Vector2 target = GetSmartRandomTarget(step); //��ʼĿ��

            while (elapsed < step.duration)
            {
                float dt = Time.deltaTime;
                elapsed += Time.deltaTime;
                perTargetElapsed += Time.deltaTime;
                coolDownTimer -= Time.deltaTime;

                if (perTargetElapsed < step.accelTime)
                    speed += accel * dt;
                else
                    speed = step.targetSpeed;

                Vector2 currentPos = transform.position;
                Vector2 dir = (target - currentPos).normalized;
                float dist = Vector2.Distance(currentPos, target);

                // ����ӽ�Ŀ�꣬�͵ȴ�һ�Ტ����Ŀ��
                if (dist < 0.05f)
                {
                    if (coolDownTimer > 0f)
                    {
                        yield return new WaitForSeconds(coolDownTimer);
                        coolDownTimer = step.coolDown;
                    }

                    target = GetSmartRandomTarget(step); // ��Ŀ��
                    speed = 0;
                    perTargetElapsed = 0f; // ���ü��ټ�ʱ
                }
                else
                {
                    transform.Translate(dir * speed * dt);
                    yield return null;
                }
            }
        }
    private Vector2 GetSmartRandomTarget(BossMovementStep step)
    {
        Vector2 playerPos = playerTransform.position;
        Vector2 bossPos = transform.position;

        // ��������ƫ����ҵ�Ŀ���
        float offsetX = Random.Range(-step.randomAreaSize.x * 0.5f, step.randomAreaSize.x * 0.5f);
        float targetX = Mathf.Clamp(playerPos.x + offsetX, -3.5f, 1.1f); // ��Ұ�߽�

        float offsetY = Random.Range(-step.randomAreaSize.y * 0.5f, step.randomAreaSize.y * 0.5f);
        float targetY = bossPos.y + offsetY;

        Vector2 rawTarget = new Vector2(targetX, targetY);

        // ����Ŀ����벻���� maxMoveRange
        float maxMoveRange = step.maxMoveRange > 0f ? step.maxMoveRange : 2.5f; // ���趨����Ĭ��2.5
        Vector2 dir = (rawTarget - bossPos);
        float dist = dir.magnitude;

        if (dist > maxMoveRange)
        {
            dir = dir.normalized * maxMoveRange;
            rawTarget = bossPos + dir;
        }

        return rawTarget;
    }
}