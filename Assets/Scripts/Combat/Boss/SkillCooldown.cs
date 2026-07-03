using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Boss")]
[TaskDescription("检测技能是否冷却完毕")]
public class SkillCooldown : BossCondition
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("技能冷却时间（秒）")]
    public SharedFloat cooldownTime = 10f;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("技能名称（用于存储冷却时间的键）")]
    public string skillName = "Skill";

    // 缓存共享变量引用
    private SharedFloat lastUsedTime;

    public override void OnAwake()
    {
        base.OnAwake();

        // 初始化共享变量
        string variableName = skillName + "LastUsed";

        // 获取变量（根据Behavior Designer版本调整）
        var variable = boss.behaviorTree.GetVariable(variableName);
        if (variable != null && variable is SharedFloat)
        {
            lastUsedTime = (SharedFloat)variable;
        }
        else
        {
            // 创建新变量
            lastUsedTime = new SharedFloat();
            lastUsedTime.Value = -Mathf.Infinity; // 初始化为负无穷，确保技能一开始可用
            boss.behaviorTree.SetVariableValue(variableName, lastUsedTime);
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (boss == null || boss.behaviorTree == null)
            return TaskStatus.Failure;

        float currentTime = Time.time;
        float lastUsed = lastUsedTime.Value;

        return (currentTime - lastUsed >= cooldownTime.Value)
            ? TaskStatus.Success
            : TaskStatus.Failure;
    }
}