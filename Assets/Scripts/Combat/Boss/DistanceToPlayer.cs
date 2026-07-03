using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Boss")]
[TaskDescription("检测与玩家的距离")]
public class DistanceToPlayer : BossCondition
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("距离阈值：小于此距离时返回成功")]
    public SharedFloat distanceThreshold = 5f;

    public override TaskStatus OnUpdate()
    {
        if (boss == null || boss.Target == null)
            return TaskStatus.Failure;

        float currentDistance = Vector2.Distance(
            boss.transform.position,
            boss.Target.position
        );

        // 距离小于阈值时返回成功，否则返回失败
        return (currentDistance <= distanceThreshold.Value)
            ? TaskStatus.Success
            : TaskStatus.Failure;
    }
}