using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// BOSS行为树动作基类（添加动画控制）
/// </summary>
public abstract class BossAction : Action
{
    [Header("BOSS对象的引用（自动获取，无需手动设置）")]
    public SharedGameObject bossGameObject;

    protected BossBase boss;
    protected Animator animator;

    public override void OnAwake()
    {// 自动获取单例BOSS（场景中只有一个BOSS时）
        if (bossGameObject.Value == null && BossBase.Instance != null)
        {
            bossGameObject.Value = BossBase.Instance.gameObject;
        }

        if (bossGameObject.Value != null)
        {
            boss = bossGameObject.Value.GetComponent<BossBase>();
            animator = bossGameObject.Value.GetComponent<Animator>();
        }
    }

    public override void OnStart()
    {
       
    }

    /// <summary>
    /// 触发动画状态
    /// </summary>
    protected void TriggerAnimation(string animationTrigger)
    {
        if (animator != null)
            animator.SetTrigger(animationTrigger);
    }

    /// <summary>
    /// 设置动画布尔参数
    /// </summary>
    protected void SetAnimationBool(string parameterName, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterName, value);
    }
}