using UnityEngine;

public class Stone : MonoBehaviour
{
    public float flySpeed = 10f;      // 飞行速度
    public float lifeTime = 3f;        // 存活时间
    private float Timer;
    Animator Animator;
    Vector2 dir;

    void Awake()
    {
        Timer = lifeTime;
        Animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
       
    }
    void Update()
    {

        // 达到存活时间后回收
        if (Timer-Time.deltaTime<=0)
        {
           Destroy(gameObject);
            return;
        } 
            transform.Translate(dir * flySpeed * Time.deltaTime);
    }

    // 初始化石头飞行方向
    public void Initialize(Vector2 targetDirection)
    {
        // 设置朝向目标
        dir = targetDirection.normalized;
    }

    // 碰撞检测（示例）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测到玩家或其他可碰撞对象时回收
        if (other.CompareTag("Player"))
        {
          
            Animator.SetTrigger("Break");
            flySpeed = 0;
        }
    }

    public void DestoryAfter() 
    {
        Destroy(gameObject);
    }
}