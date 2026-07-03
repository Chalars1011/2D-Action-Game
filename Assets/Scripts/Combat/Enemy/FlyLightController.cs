using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyLightController : MonoBehaviour
{
    public float flySpeed = 10f;      // 飞行速度
    public float lifeTime = 3f;        // 存活时间
    private float Timer;
    private Vector3 moveDirection = Vector3.right; // 默认向右移动
    Animator Animator;
    Vector2 dir;

    void Awake()
    {
       
       
    }

    private void OnEnable()
    {
        Timer = lifeTime;
        Animator = GetComponent<Animator>();
    }
    void Update()
    {
        // 倒计时
        if (Timer - Time.deltaTime <= 0)
        {
            PoolManager.Instance.PushObj(gameObject.name, gameObject);
            return;
        }

        // 向前移动（水平方向）
        transform.Translate(moveDirection * flySpeed * Time.deltaTime);

        // 更新计时器
        Timer -= Time.deltaTime;
    }
    public void Initialize( int face)
    {
        transform.localScale = new Vector3(face, transform.localScale.y, transform.localScale.z);
        // 根据face设置移动方向
        moveDirection = face == 1 ? Vector3.right : Vector3.left;
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
        PoolManager.Instance.PushObj(gameObject.name, gameObject);
    }
}
