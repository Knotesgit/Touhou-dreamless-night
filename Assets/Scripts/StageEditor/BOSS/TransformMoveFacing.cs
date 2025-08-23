using UnityEngine;

public class TransformMoveFacing : MonoBehaviour
{
    public Animator animator;       // 绑定 Animator
    public string speedParam = "Speed"; // Animator 中的速度参数名

    public float deadZone = 0.02f;  // X方向死区（防止抖动）

    private Vector3 lastPos;
    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 delta = transform.position - lastPos;
        float speed = delta.magnitude / Time.deltaTime; // 位移量转速度
        // ---- 朝向检测（只看X位移） ----
        if (delta.x < 0) 
        {
            speed = -speed;
        }

        if (animator)
        {
            if (!string.IsNullOrEmpty(speedParam))
                animator.SetFloat(speedParam, speed);
        }

        lastPos = transform.position;
    }
}
