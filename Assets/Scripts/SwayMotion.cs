using UnityEngine;

public class SwayMotion : MonoBehaviour
{
    [Header("基础方向")]
    [Tooltip("移动方向角度（单位：度，0为右，90为上）")]
    [Range(0f, 360f)]
    public float moveAngle = 0f;

    [Tooltip("移动速度")]
    public float moveSpeed = 1f;

    [Header("Sway配置")]
    [Tooltip("Sway 振幅（最大偏转角度）")]
    public float swayAmplitude = 15f;

    [Tooltip("Sway 频率（Hz）")]
    public float swayFrequency = 1f;

    [Tooltip("是否在旋转后朝向运动方向（用于精灵对齐）")]
    public bool rotateToDirection = false;

    private Vector2 initialDirection;
    private float swayTimer = 0f;

    void Start()
    {
        float rad = moveAngle * Mathf.Deg2Rad;
        initialDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    void Update()
    {
        swayTimer += Time.deltaTime;
        float swayAngle = Mathf.Sin(swayTimer * swayFrequency * Mathf.PI * 2f) * swayAmplitude;
        Vector2 swayDirection = Rotate(initialDirection, swayAngle);
        transform.Translate(swayDirection * moveSpeed * Time.deltaTime, Space.World);

        if (rotateToDirection)
            transform.right = swayDirection;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
   
    }
}
