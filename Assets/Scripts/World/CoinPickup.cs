using GameArchitecture.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private bool isCollected = false;
    [Header("停留设置")]
    public float minStayTime = 0.5f;
    public float maxStayTime = 1.5f;

    [Header("移动设置")]
    public float minMoveSpeed = 3f;
    public float maxMoveSpeed = 6f;

    [Header("目标设置")]
    public string playerName = "Player";
    private float currentStayTime;
    private float moveSpeed;
    private bool isMoving = false;
    private Transform playerTransform;
    private Collider2D coinCollider;
    private Animator anim;

    private void Awake()
    {
       

    }

    private void OnEnable()
    {

        coinCollider = GetComponent<Collider2D>();

        // 确保初始时碰撞体是关闭的
        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }
        anim = GetComponent<Animator>();
        bool isCollected = false;
        // 重置状态
        isMoving = false;

        // 随机停留时间
        currentStayTime = Random.Range(minStayTime, maxStayTime);

        // 随机移动速度
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

        // 查找玩家
        GameObject player = Blackboard.PlayerTransform?.gameObject;
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"未找到名为 {playerName} 的玩家对象");
        }
        anim.SetBool("Destory", false);
    }

    private void Update()
    {
        // 停留阶段
        if (!isMoving)
        {
            currentStayTime -= Time.deltaTime;
            if (currentStayTime <= 0 && playerTransform != null)
            {
                isMoving = true;
            }
            return;
        }

        // 移动阶段
        if (playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position + new Vector3(0, 2, 0),
                moveSpeed * Time.deltaTime
            );

            // 检查是否到达玩家位置
            if (Vector2.Distance(transform.position, playerTransform.position + new Vector3(0, 2, 0)) < 0.1f)
            {
                // 到达玩家位置，激活碰撞体
                if (coinCollider != null)
                {
                    coinCollider.enabled = true;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            // 标记为已收集
            isCollected = true;

            // 增加玩家的金币数量
            PlayerItemManager.Instance.AddCurrency("Gold", 1);

            // 播放销毁动画
            anim.SetBool("Destory", true);

        }
    }

    private void PuShObjct() 
    {
        // 回收金币到对象池
        PoolManager.Instance.PushObj("JinBi", gameObject);
    }
}

