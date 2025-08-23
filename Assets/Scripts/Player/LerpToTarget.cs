using UnityEngine;

public class LerpToTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;         // Ҫ�����Ŀ��
    public float lerpSpeed = 10f;    // ��ֵ�ٶȣ�Խ��Խ�죩

    [Header("Options")]
    public bool followRotation = false; // �Ƿ�ͬ����ת
    public bool followPosition = true;  // �Ƿ�ͬ��λ��

    void Update()
    {
        if (target == null) return;

        // λ�ò�ֵ
        if (followPosition)
        {
            if (Vector3.Distance(transform.position, target.position) > 1f)
            {
                transform.position = target.position;
            }
            else 
            {
                transform.position = Vector3.Lerp(
                transform.position,
                target.position,
                lerpSpeed * Time.deltaTime);
            }
        }

        // ��ת��ֵ
        if (followRotation)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                target.rotation,
                lerpSpeed * Time.deltaTime
            );
        }
    }
}
