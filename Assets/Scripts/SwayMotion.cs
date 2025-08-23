using UnityEngine;

public class SwayMotion : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("�ƶ�����Ƕȣ���λ���ȣ�0Ϊ�ң�90Ϊ�ϣ�")]
    [Range(0f, 360f)]
    public float moveAngle = 0f;

    [Tooltip("�ƶ��ٶ�")]
    public float moveSpeed = 1f;

    [Header("Sway����")]
    [Tooltip("Sway ��������ƫת�Ƕȣ�")]
    public float swayAmplitude = 15f;

    [Tooltip("Sway Ƶ�ʣ�Hz��")]
    public float swayFrequency = 1f;

    [Tooltip("�Ƿ�����ת�����˶��������ھ�����룩")]
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
