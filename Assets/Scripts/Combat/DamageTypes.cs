using UnityEngine;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 伤害类型——决定哪些抗性/弱点生效。
    /// </summary>
    public enum DamageType
    {
        Physical,    // 物理伤害（普通攻击）
        Skill,       // 技能伤害
        Poison,      // 毒 DOT
        Fire,        // 火焰
        Ice,         // 冰冻
        Lightning,   // 雷电
        True         // 真实伤害——无视一切抗性
    }

    /// <summary>
    /// 伤害计算阶段。
    /// 攻击方增益和防御方抗性分开处理，支持不同的叠加规则。
    /// </summary>
    public enum ModPhase
    {
        AttackerBonus,   // 攻击方增益（Buff、装备、技能倍率）
        DefenderResist,  // 防御方抗性（护甲、减伤、属性克制）
        HitPart          // 命中部位修正（爆头、弱点）
    }

    /// <summary>
    /// 伤害计算的输入——Calculator 只需要这些数据，
    /// 不依赖 GameObject 或 MonoBehaviour。
    /// </summary>
    public struct DamageInput
    {
        public float baseDamage;           // 原始伤害值
        public DamageType damageType;      // 伤害类型
        public int attackerInstanceId;     // 攻击者实例 ID
        public int targetInstanceId;       // 目标实例 ID
        public bool isHeavyHit;            // 是否重击
        public Vector2 knockbackDirection; // 击退方向

        public static DamageInput FromAttack(AttackBase attacker, global::Character target)
        {
            return new DamageInput
            {
                baseDamage = attacker.atk,
                damageType = DamageType.Physical,
                attackerInstanceId = attacker.GetInstanceID(),
                targetInstanceId = target.GetInstanceID(),
                isHeavyHit = attacker.isHeavyHit,
                knockbackDirection = (target.transform.position - attacker.transform.position).normalized
            };
        }

        public static DamageInput FromCollision(EnemyCollisionDamage source, global::Character target)
        {
            return new DamageInput
            {
                baseDamage = source.damage,
                damageType = DamageType.Physical,
                attackerInstanceId = source.GetInstanceID(),
                targetInstanceId = target.GetInstanceID(),
                isHeavyHit = false,
                knockbackDirection = (target.transform.position - source.transform.position).normalized
            };
        }
    }

    /// <summary>
    /// 伤害计算的完整结果——包含最终伤害和调试信息。
    /// </summary>
    public struct DamageResult
    {
        public int finalDamage;            // 最终伤害值
        public float rawDamage;            // 应用修正前的伤害
        public int modifierCount;          // 应用了多少个修正器
        public bool targetDied;            // 目标是否因此死亡
        public string debugInfo;           // 调试信息

        public override string ToString()
        {
            return $"Damage: {rawDamage:F0} → {finalDamage} ({modifierCount} mods){(targetDied ? " [KILL]" : "")}";
        }
    }

    /// <summary>
    /// QueryBus 查询：请求所有系统提供伤害修正倍率。
    /// 每个注册的 Handler 返回自己的倍率（1.0 = 不影响）。
    /// </summary>
    public struct DamageModQuery
    {
        public ModPhase phase;             // 当前阶段
        public float baseDamage;           // 原始伤害
        public float currentDamage;        // 当前阶段开始时的伤害
        public DamageType damageType;      // 伤害类型
        public int attackerInstanceId;     // 攻击者
        public int targetInstanceId;       // 目标
    }
}
