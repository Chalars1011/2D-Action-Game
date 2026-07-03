using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// BOSS行为树条件基类（简化版）
/// </summary>
public abstract class BossCondition : Conditional
{
    [Header("BOSS对象的引用")]
    public SharedGameObject bossGameObject;


    protected BossBase boss;

    public override void OnAwake()
    {
        // 优先使用手动设置的值，否则使用单例
        if (bossGameObject.Value == null && BossBase.Instance != null)
        {
            bossGameObject.Value = BossBase.Instance.gameObject;
        }

        if (bossGameObject.Value != null)
            boss = bossGameObject.Value.GetComponent<BossBase>();
    }

    public override void OnStart()
    {
        if (boss == null)
            Debug.LogError($"[{nameof(BossCondition)}] 未找到Boss组件，请检查BOSS对象是否正确赋值！");
    }
}