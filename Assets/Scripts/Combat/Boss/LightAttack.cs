using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Boss/Skill")]
public class LightAttack : BossAction
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("轻攻击动画状态名")]
    public string animStateName = "LightAttack";

    [BehaviorDesigner.Runtime.Tasks.Tooltip("攻击持续时间（秒）")]
    public float attackDuration = 1.5f;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("是否在攻击结束后自动重置动画")]
    public bool resetAnimationOnEnd = true;

    private float startTime;
    private bool animationTriggered;

    public override void OnStart()
    {
        base.OnStart();

        // 记录攻击开始时间
        startTime = Time.time;
        animationTriggered = false;

        // 触发攻击动画
        if (animator != null && !string.IsNullOrEmpty(animStateName))
        {
            TriggerAnimation(animStateName);
            animationTriggered = true;
       
        }
    }

    public override TaskStatus OnUpdate()
    {
        // 计算已持续时间
        float elapsedTime = Time.time - startTime;

        // 检查是否达到攻击持续时间
        if (elapsedTime >= attackDuration)
        {
       
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        base.OnEnd();

        // 重置动画状态（可选）
        if (resetAnimationOnEnd && animator != null && animationTriggered)
        {
            animator.ResetTrigger(animStateName);
            SetAnimationBool("IsAttacking", false);
    
        }
    }
}