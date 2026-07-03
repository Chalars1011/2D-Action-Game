using UnityEngine;
using UnityEngine.InputSystem;

public class ShenKan : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnPoint;
    [SerializeField] private int faceDirection = 1; // 1表示向右，-1表示向左

    [SerializeField]
    private bool isPlayerInRange = false;
    PlayerInputControl inputControl;

    private void Awake()
    {
        inputControl = new PlayerInputControl();
        inputControl.GamePlay.JiaoHu.started += ctx => OnInteractStarted();
    }
    private void Start()
    {
      
    }

    private void OnEnable()
    {
        inputControl.Enable();
    }

    private void OnDisable()
    {
        inputControl.Disable();
    }

    private void OnInteractStarted()
    {
       
        // 仅当玩家在范围内时执行交互
        if (isPlayerInRange)
        {
            InteractWithShenKan();
          
        }
    }

    private void InteractWithShenKan()
    {
        Debug.Log("玩家与神龛交互！");

        // 调用传送门管理器进行场景跳转
        if (PortalManager.Instance != null)
        {
            PortalManager.Instance.Teleport(targetScene, targetSpawnPoint, faceDirection);
        }
        else
        {
            Debug.LogError("PortalManager实例不存在！", gameObject);
        }
    }

    // 2D碰撞检测：使用OnTriggerEnter2D替代OnTriggerEnter
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            Debug.Log("玩家进入神龛交互范围！");

            // 这里可以添加提示UI显示逻辑
            ShowInteractionPrompt();
        }
    }

    // 2D碰撞检测：使用OnTriggerExit2D替代OnTriggerExit
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            Debug.Log("玩家离开神龛交互范围！");

            // 这里可以添加提示UI隐藏逻辑
            HideInteractionPrompt();
        }
    }

    // 显示交互提示（示例方法，需要实现具体UI逻辑）
    private void ShowInteractionPrompt()
    {
        // 实现显示交互提示的逻辑（如显示"按E交互"文本）
    }

    // 隐藏交互提示（示例方法，需要实现具体UI逻辑）
    private void HideInteractionPrompt()
    {
        // 实现隐藏交互提示的逻辑
    }
}