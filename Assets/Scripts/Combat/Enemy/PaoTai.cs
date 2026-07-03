using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaoTai : MonoBehaviour
{
    [Header("炮塔设置")]
    public Transform firePoint;           // 发射点
    public GameObject projectilePrefab;   // 炮弹预制体
    public float fireInterval = 3f;       // 发射间隔
    public Vector2 fireDirection = Vector2.right; // 发射方向（默认向右）
    private Animator Animator;
    private float fireTimer;

    void Start()
    {
        fireTimer = fireInterval;
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 冷却计时
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }
        else
        {
            Fire();
            fireTimer = fireInterval;
        }
    }

    // 发射炮弹
    private void Fire()
    {
        if (firePoint == null || projectilePrefab == null)
            return;
        Animator.SetTrigger("shoot");
    }


    private void shootPaoDan() 
    {

        // 实例化炮弹预制体
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // 获取炮弹脚本并设置方向
        PaoDan projectile = projectileObj.GetComponent<PaoDan>();
        if (projectile != null)
        {
            projectile.Initialize(fireDirection.normalized);
        }

    }

    // 可视化发射方向
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (firePoint != null)
        {
            Gizmos.DrawRay(firePoint.position, fireDirection * 3f);
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}
