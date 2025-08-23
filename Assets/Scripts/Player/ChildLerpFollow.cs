using UnityEngine;

public class ChildLerpFollow : MonoBehaviour
{
    public Transform parentOverride;         // 可空：不填就用 transform.parent
    public float followSpeed = 12f;          // 10~25 合理
    public bool ignoreParentScale = true;    // 父缩放会拉扯 offset 的话，建议忽略
    public bool followRotation = false;      // 需要就插值旋转

    Vector3 localOffset;                     // 起始的“父空间”偏移
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
        // 记录初始相对关系
        localOffset = transform.position - p.position;
        localRotOffset = Quaternion.Inverse(p.rotation) * transform.rotation;
    }

    void LateUpdate()
    {
        var p = parentOverride ? parentOverride : transform.parent;
        if (!p) return;

        // 计算期望世界位置（可选是否忽略父缩放）
        Vector3 desiredPos = ignoreParentScale
            ? p.position + p.rotation * localOffset                // 只受旋转影响
            : p.TransformPoint(p.InverseTransformDirection(localOffset)); // 完整父变换

        // 位置插值到期望世界坐标
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
