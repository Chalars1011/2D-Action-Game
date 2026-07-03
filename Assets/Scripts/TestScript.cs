using UnityEngine;

/// <summary>
/// 这是测试Git用的脚本，演示暂存、提交、推送、回退功能。
/// 你可以随时删掉。
/// </summary>
public class TestScript : MonoBehaviour
{
    [Header("测试参数")]
    public string testName = "Git测试";
    public int testValue = 100;
    public bool testFlag = true;

    void Start()
    {
        Debug.Log($"Git测试脚本启动：{testName}, 值={testValue}");
        // 【同事小王加的】初始化玩家血量
        Debug.Log("同事小王：在Start方法里加了玩家血量初始化逻辑");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("你按了空格键 —— 这是第一版测试方法");
        }
    }
}
