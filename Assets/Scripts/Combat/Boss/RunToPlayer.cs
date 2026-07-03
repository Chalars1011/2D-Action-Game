using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Boss")]
[TaskDescription("使BOSS向玩家跑动")]
public class RunToPlayer : BossAction
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("目标距离阈值：到达此距离时停止移动")]
    public SharedFloat targetDistance = 2f;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("是否在移动时持续检查朝向")]
    public bool checkFacingDuringMove = true;

    public override void OnStart()
    {
        base.OnStart();
        SetAnimationBool("IsRunning", true); // 开始跑动动画
    }

    public override TaskStatus OnUpdate()
    {
        if (boss == null || boss.Target == null)
            return TaskStatus.Failure;

        // 1. 检查是否已到达目标距离
        if (IsDistanceValid())
        {
            return TaskStatus.Success; // 距离够了，退出移动
        }

        // 2. 检查是否面向玩家（可配置是否在移动时检查）
        if (checkFacingDuringMove && !CheckIsFacingPlayer())
        {
            // 不面向时，可选择停止移动或调整朝向
            return TaskStatus.Failure;
        }

        // 3. 执行移动逻辑
        Vector2 direction = (boss.Target.position - boss.transform.position).normalized;
        boss.rb.velocity = direction * boss.moveSpeed;

        return TaskStatus.Running; // 持续执行
    }

    public override void OnEnd()
    {
        boss.rb.velocity = Vector2.zero; // 停止移动
        SetAnimationBool("IsRunning", false); // 停止跑动动画
    }

    // 复用 DistanceToPlayer 的核心逻辑
    private bool IsDistanceValid()
    {
        if (boss == null || boss.Target == null)
            return false;

        float currentDistance = Vector2.Distance(
            boss.transform.position,
            boss.Target.position
        );

        // 距离小于阈值时返回成功
        return currentDistance <= targetDistance.Value;
    }

    // 复用 IsFacingPlayer 的核心逻辑
    private bool CheckIsFacingPlayer()
    {
        Vector2 bossToPlayer = (boss.Target.position - boss.transform.position).normalized;
        float bossFacing = Mathf.Sign(boss.transform.localScale.x);

        // 方向一致判定
        return (bossToPlayer.x > 0 && bossFacing > 0)
            || (bossToPlayer.x < 0 && bossFacing < 0);
    }
}