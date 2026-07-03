using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Boss")]
[TaskDescription("触发死亡动画并禁用BOSS")]
public class Die : BossAction
{
    [Header("死亡动画名称")]
    public string deathAnimation = "Death";

    public override TaskStatus OnUpdate()
    {
        if (boss == null) return TaskStatus.Failure;

        // 禁用碰撞体和移动
        boss.GetComponent<Collider2D>().enabled = false;
        boss.rb.velocity = Vector2.zero;

        // 触发死亡动画
        TriggerAnimation(deathAnimation);

        // 禁用行为树
        if (boss.behaviorTree != null)
            boss.behaviorTree.DisableBehavior();

        return TaskStatus.Success;
    }
}