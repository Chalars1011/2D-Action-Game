using GameArchitecture.Core;
using UnityEngine;
using Cinemachine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("玩家设置")]
    [Tooltip("要跟随的玩家对象名称")]
    [SerializeField] private string playerName = "Player";

    [Header("自动查找设置")]
    [Tooltip("游戏开始时是否自动查找玩家对象")]
    [SerializeField] private bool autoFindPlayer = true;

    private CinemachineVirtualCamera virtualCamera;
    private GameObject player;

    private void Awake()
    {
        // 获取虚拟相机组件
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("未找到CinemachineVirtualCamera组件！", gameObject);
            return;
        }
    }

    private void Start()
    {
        // 如果启用了自动查找，则查找玩家
        if (autoFindPlayer)
        {
            FindPlayer();
        }

        // 设置相机跟随目标
        SetFollowTarget();
    }

    private void FindPlayer()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            return;
        }

        player = Blackboard.PlayerTransform?.gameObject;

    }

    private void SetFollowTarget()
    {
        if (player != null && virtualCamera != null)
        {
            // 设置相机跟随目标
            virtualCamera.Follow = player.transform;

            // virtualCamera.LookAt = player.transform;
        }
    }
}