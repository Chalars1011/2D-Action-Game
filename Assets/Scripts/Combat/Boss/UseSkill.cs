using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Boss")]
[TaskDescription("触发技能动画并更新冷却时间")]
public class UseSkill : BossAction
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("技能动画名称")]
    public string skillAnimation = "Skill";

    [BehaviorDesigner.Runtime.Tasks.Tooltip("技能名称（与冷却节点一致）")]
    public string skillName = "Skill";

    public override TaskStatus OnUpdate()
    {
        if (boss == null || boss.behaviorTree == null)
            return TaskStatus.Failure;

        TriggerAnimation(skillAnimation);

        // 更新冷却时间
        UpdateCooldown(skillName);

        return TaskStatus.Success;
    }

    private void UpdateCooldown(string skillName)
    {
        string variableName = skillName + "LastUsed";

        // 获取变量
        var variable = boss.behaviorTree.GetVariable(variableName);
        if (variable != null && variable is SharedFloat)
        {
            SharedFloat lastUsedTime = (SharedFloat)variable;
            lastUsedTime.Value = Time.time;
            boss.behaviorTree.SetVariableValue(variableName, lastUsedTime);
        }
        else
        {
            Debug.LogError($"[{nameof(UseSkill)}] 未找到技能冷却变量: {variableName}");
        }
    }
}