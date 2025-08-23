using UnityEngine;

public class TransformMoveFacing : MonoBehaviour
{
    public Animator animator;       // �� Animator
    public string speedParam = "Speed"; // Animator �е��ٶȲ�����

    public float deadZone = 0.02f;  // X������������ֹ������

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
        float speed = delta.magnitude / Time.deltaTime; // λ����ת�ٶ�
        // ---- �����⣨ֻ��Xλ�ƣ� ----
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
