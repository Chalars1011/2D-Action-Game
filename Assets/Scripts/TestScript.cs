using UnityEngine;

public class TestScript : MonoBehaviour
{
    [Header("测试参数")]
    public string testName = "Git测试";
    public int testValue = 100;

    void Start()
    {
        // 【坏代码】这行会导致游戏直接崩溃
        GameObject.Find(null);
        
        // 【坏代码】除以零
        int result = testValue / 0;
        
        Debug.Log("这行永远执行不到，因为上面已经崩溃了");
    }
}
