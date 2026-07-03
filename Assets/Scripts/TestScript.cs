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
    }

    void Update()
    {
        // 测试方法1：按空格键输出信息
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("你按了空格键 —— 这是第一版测试方法");
        }
    }
}
