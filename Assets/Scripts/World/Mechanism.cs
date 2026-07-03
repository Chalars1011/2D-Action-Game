using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanism : MonoBehaviour
{

    private Animator animator;

    [Header("机关设置")]
    public Transform targetObject;        // 要移动的物体
    public float moveDistance = 5f;       // 移动距离
    public float moveSpeed = 2f;          // 移动速度
    public Vector2 moveDirection = Vector2.up; // 移动方向（默认向上）

    [Header("检测设置")]
    public string playerTag = "Player";   // 玩家标签

    private bool isPlayerNear = false;    // 玩家是否在附近
    private bool isActivated = false;     // 机关是否已激活
    private Vector3 originalPosition;     // 物体原始位置
    private Vector3 targetPosition;       // 目标位置

    void Start()
    {
        animator = GetComponent<Animator>();
        if (targetObject != null)
        {
            originalPosition = targetObject.position;
            targetPosition = originalPosition + (Vector3)(moveDirection.normalized * moveDistance);
        }
    }

    void Update()
    {
        // 如果机关已激活，移动物体
        if (isActivated && targetObject != null)
        {
            MoveTargetObject();
        }
    }

    // 移动目标物体
    private void MoveTargetObject()
    {
        // 平滑移动到目标位置
        targetObject.position = Vector3.MoveTowards(targetObject.position, targetPosition, moveSpeed * Time.deltaTime);

        // 检查是否到达目标位置
        if (Vector3.Distance(targetObject.position, targetPosition) < 0.01f)
        {
            targetObject.position = targetPosition;
            isActivated = false; // 到达目标位置后停止移动
        }
    }

    // 触发机关
    public void Activate()
    {
        if (isPlayerNear && !isActivated)
        {
            isActivated = true;
        }
        animator.SetTrigger("open");
    }

    // 重置机关
    public void Reset()
    {
        isActivated = false;
        if (targetObject != null)
        {
            targetObject.position = originalPosition;
        }
    }

    // 碰撞检测：玩家进入触发范围
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = true;
            // 可以在这里显示交互提示UI
            ShowInteractionPrompt();
        }
    }

    // 碰撞检测：玩家离开触发范围
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = false;
            // 可以在这里隐藏交互提示UI
            HideInteractionPrompt();
        }
    }

    // 显示交互提示
    private void ShowInteractionPrompt()
    {
        Debug.Log("玩家靠近机关，按E触发");
    }

    // 隐藏交互提示
    private void HideInteractionPrompt()
    {
        Debug.Log("玩家离开机关范围");
    }

    // 可视化移动范围
    private void OnDrawGizmosSelected()
    {
        if (targetObject != null)
        {
            Gizmos.color = Color.blue;
            Vector3 targetPos = targetObject.position + (Vector3)(moveDirection.normalized * moveDistance);
            Gizmos.DrawLine(targetObject.position, targetPos);
            Gizmos.DrawWireSphere(targetPos, 0.2f);
        }

        // 绘制触发范围
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
