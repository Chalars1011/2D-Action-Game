using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Boss")]
[TaskDescription("使BOSS面向玩家")]
public class FacePlayer : BossAction
{
    public override TaskStatus OnUpdate()
    {
        if (boss == null || boss.Target == null)
            return TaskStatus.Failure;

        // 计算朝向
        Vector2 direction = (boss.Target.position - boss.transform.position).normalized;

        // 根据X轴方向设置朝向
        if (direction.x > 0.1f) // 向右
            boss.transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < -0.1f) // 向左
            boss.transform.localScale = new Vector3(-1, 1, 1);

        return TaskStatus.Success;
    }
}