using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

[TaskCategory("Boss")]
[TaskDescription("检测BOSS当前生命值百分比")]
public class HealthPercentage : BossCondition
{
    [Tooltip("比较模式（小于/大于/等于）")]
    public CompareMode compareMode = CompareMode.LessThan;

    [Tooltip("生命值百分比阈值")]
    public SharedFloat threshold = 0.5f;

    public override TaskStatus OnUpdate()
    {
        if (boss == null || !boss.IsAlive)
            return TaskStatus.Failure;

        float healthPercent = boss.character.currentHealth / boss.character.maxHealth;

        switch (compareMode)
        {
            case CompareMode.LessThan:
                return healthPercent < threshold.Value ? TaskStatus.Success : TaskStatus.Failure;
            case CompareMode.GreaterThan:
                return healthPercent > threshold.Value ? TaskStatus.Success : TaskStatus.Failure;
            case CompareMode.EqualTo:
                return UnityEngine.Mathf.Abs(healthPercent - threshold.Value) < 0.01f ? TaskStatus.Success : TaskStatus.Failure;
            default:
                return TaskStatus.Failure;
        }
    }

    public enum CompareMode
    {
        LessThan,
        GreaterThan,
        EqualTo
    }
}