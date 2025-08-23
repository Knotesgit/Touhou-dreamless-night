using UnityEngine;

public class BombEffect : MonoBehaviour
{
    
    public float moveSpeed = 10f;
    public float rotateSpeed = 180f; // ÿ����ת�Ƕ�
    public float dps = 10f;
    public AudioClip shoot;
    private bool following = true;
    private Vector2 direction = Vector2.up;
    private bool fired = false;
    private GameObject target; // ��� 

    private void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }
    private void Start()
    {
        
    }
    void Update()
    {
        if (following && target != null)
        {
            // �������� / ��ֵ������ɣ�ȡ��������ҪʲôЧ��
            //transform.position = target.transform.position;
            // ����ƽ�����棺
            transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime * moveSpeed);
        }
        if (fired)
        {
            transform.position += (Vector3)(direction.normalized * moveSpeed * Time.deltaTime);
        }
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        if (transform.position.y > 5) 
        {
            Destroy(gameObject);    
        }
    }

    public void Fire()
    {
        following = false;
        // ���Բ��ŷ��䶯��/���ӵ�
        fired = true;
        GeneralAudioPool.Instance.PlayOneShot(shoot);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null) 
        {
            bullet.BombDestroy();
        }
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        IHittable enemy = collision.GetComponent<IHittable>();
        if (enemy != null) 
        {
            enemy.OnHit(dps*Time.deltaTime);
        }
    }
}
