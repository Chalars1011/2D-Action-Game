using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollisionDamage : MonoBehaviour
{


    [Header("碰撞设置")]
    public Collider2D damageCollider; // 可选：指定用于检测碰撞的碰撞体
    public float damage;
    public string targetTag = "Player"; // 目标标签
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // 物理碰撞检测（非触发器）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否为目标标签
        if (!collision.gameObject.CompareTag(targetTag)) return;

        // 执行伤害方法
        collision.gameObject.GetComponent<Character>()?.TakeDmageTouch(this);

       // Debug.Log($"[碰撞伤害] 敌人: {gameObject.name} 碰撞到 玩家: {collision.gameObject.name}, 执行伤害");
    }

}
