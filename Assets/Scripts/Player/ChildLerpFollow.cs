using UnityEngine;

public class ChildLerpFollow : MonoBehaviour
{
    public Transform parentOverride;         // �ɿգ�������� transform.parent
    public float followSpeed = 12f;          // 10~25 ����
    public bool ignoreParentScale = true;    // �����Ż����� offset �Ļ����������
    public bool followRotation = false;      // ��Ҫ�Ͳ�ֵ��ת

    Vector3 localOffset;                     // ��ʼ�ġ����ռ䡱ƫ��
    Quaternion localRotOffset;

    void Start()
    {
        var p = parentOverride ? parentOverride : transform.parent;
        if (p == null)
        {
            Debug.LogWarning("ChildLerpFollow: no parent.");
            enabled = false;
            return;
        }
        // ��¼��ʼ��Թ�ϵ
        localOffset = transform.position - p.position;
        localRotOffset = Quaternion.Inverse(p.rotation) * transform.rotation;
    }

    void LateUpdate()
    {
        var p = parentOverride ? parentOverride : transform.parent;
        if (!p) return;

        // ������������λ�ã���ѡ�Ƿ���Ը����ţ�
        Vector3 desiredPos = ignoreParentScale
            ? p.position + p.rotation * localOffset                // ֻ����תӰ��
            : p.TransformPoint(p.InverseTransformDirection(localOffset)); // �������任

        // λ�ò�ֵ��������������
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );

        if (followRotation)
        {
            var desiredRot = p.rotation * localRotOffset;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRot,
                followSpeed * Time.deltaTime
            );
        }
    }
}
