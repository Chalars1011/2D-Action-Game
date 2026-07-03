using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Boss")]
[TaskDescription("触发重攻击动画")]
public class HeavyAttack : BossAction
{
    [Tooltip("重攻击动画名称")]
    public string heavyAttackAnimation = "HeavyAttack";

    public override TaskStatus OnUpdate()
    {
        if (boss == null) return TaskStatus.Failure;

       // boss.StartAttack(); // 通知BOSS开始攻击状态
        TriggerAnimation(heavyAttackAnimation); // 触发动画

        return TaskStatus.Success;
    }
}