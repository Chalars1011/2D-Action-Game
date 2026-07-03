using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
[TaskDescription("检测BOSS是否面向玩家")]
public class IsFacingPlayer : BossCondition
{
    public override TaskStatus OnUpdate()
    {
        if (boss == null || boss.character == null)
            return TaskStatus.Failure;

        // 1. 计算Boss到玩家的方向并归一化
        Vector2 bossToPlayer = (boss.Target.position - boss.transform.position).normalized;

        // 2. 获取Boss当前朝向
        float bossFacingDirection = boss.transform.localScale.x;

        if (bossToPlayer.x < 0 && bossFacingDirection < 0 ||bossToPlayer.x>0&&bossFacingDirection>0) 
        {
            return TaskStatus.Success; // 朝向正确
        }
        else 
        {
            return TaskStatus.Failure; // 需要转身

        }
       
             
            
    }
}