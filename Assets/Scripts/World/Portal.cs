using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("传送设置")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnPoint;
    [SerializeField] private int faceDirection = 1; // 1表示向右，-1表示向左

    [Header("配置")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            PortalManager.Instance.Teleport(targetScene, targetSpawnPoint, faceDirection);
        }
    }
}
