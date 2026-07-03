using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    [Header("要开关的陷阱碰撞体")]
    public Collider2D trapCollider;

    [Header("检测刷新间隔（秒）")]
    public float checkInterval = 0.5f;

    [Header("触发玩家的标签")]
    public string playerTag = "Player";

    private bool isPlayerInside;

    private void Start()
    {
        if (trapCollider != null)
            trapCollider.enabled = false;

        InvokeRepeating(nameof(RefreshTrapState), 0f, checkInterval);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            isPlayerInside = false;
    }

    private void RefreshTrapState()
    {
        if (trapCollider == null) return;
        trapCollider.enabled = isPlayerInside;
    }
}
