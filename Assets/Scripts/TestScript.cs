using UnityEngine;

public class TestScript : MonoBehaviour
{
    [Header("测试参数")]
    public string testName = "Git测试";
    public int testValue = 100;
    public bool testFlag = true;

    void Start()
    {
        Debug.Log($"Git测试脚本启动：{testName}, 值={testValue}");
        Debug.Log("同事小王：掉了新代码一一致命伤害判定");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("你按了空格键");
        }
    }
}
