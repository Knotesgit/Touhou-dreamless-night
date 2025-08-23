using UnityEngine;

public class LerpToTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;         // 要跟随的目标
    public float lerpSpeed = 10f;    // 插值速度（越大越快）

    [Header("Options")]
    public bool followRotation = false; // 是否同步旋转
    public bool followPosition = true;  // 是否同步位置

    void Update()
    {
        if (target == null) return;

        // 位置插值
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

        // 旋转插值
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
