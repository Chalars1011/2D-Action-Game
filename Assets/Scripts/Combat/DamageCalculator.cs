using UnityEngine;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 伤害计算器——通过 Modifier Chain + QueryBus 计算最终伤害。
    /// 
    /// 设计原则：
    /// 1. Calculator 是纯静态方法，无状态，无副作用
    /// 2. 所有倍率来源通过 QueryBus 收集
    /// 3. Calculator 不 import 任何具体游戏模块
    /// 4. 三个计算阶段：攻击方增益 → 防御方抗性 → 部位修正
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 计算最终伤害。
        /// 
        /// 流程：
        /// 1. 查询所有攻击方增益（装备、Buff、技能...）
        /// 2. 查询所有防御方抗性（护甲、减伤、属性克制...）
        /// 3. 查询部位修正（爆头 2x、装甲 0.3x...）
        /// 4. 钳制到 [1, maxHealth]
        /// </summary>
        public static DamageResult Calculate(DamageInput input, float targetCurrentHealth, float targetMaxHealth)
        {
            float damage = input.baseDamage;
            int modCount = 0;

            // === 阶段 1：攻击方增益 ===
            var attackerMods = QueryBus.Collect<DamageModQuery, float>(
                new DamageModQuery
                {
                    phase = ModPhase.AttackerBonus,
                    baseDamage = input.baseDamage,
                    currentDamage = damage,
                    damageType = input.damageType,
                    attackerInstanceId = input.attackerInstanceId,
                    targetInstanceId = input.targetInstanceId
                }
            );
            foreach (var mod in attackerMods)
            {
                if (mod != 1.0f)
                {
                    damage *= mod;
                    modCount++;
                }
            }

            // === 阶段 2：防御方抗性 ===
            var defenderMods = QueryBus.Collect<DamageModQuery, float>(
                new DamageModQuery
                {
                    phase = ModPhase.DefenderResist,
                    baseDamage = input.baseDamage,
                    currentDamage = damage,
                    damageType = input.damageType,
                    attackerInstanceId = input.attackerInstanceId,
                    targetInstanceId = input.targetInstanceId
                }
            );
            foreach (var mod in defenderMods)
            {
                if (mod != 1.0f)
                {
                    damage *= mod;
                    modCount++;
                }
            }

            // === 阶段 3：部位修正 ===
            var partMods = QueryBus.Collect<DamageModQuery, float>(
                new DamageModQuery
                {
                    phase = ModPhase.HitPart,
                    currentDamage = damage,
                    damageType = input.damageType,
                    attackerInstanceId = input.attackerInstanceId,
                    targetInstanceId = input.targetInstanceId
                }
            );
            foreach (var mod in partMods)
            {
                if (mod != 1.0f)
                {
                    damage *= mod;
                    modCount++;
                }
            }

            // === 最终钳制 ===
            float rawDamage = damage;
            damage = Mathf.Max(1, Mathf.Round(damage));
            // 不超出目标当前血量（不会过度击杀）
            damage = Mathf.Min(damage, targetCurrentHealth);

            int finalDamage = (int)damage;
            bool targetDied = (targetCurrentHealth - finalDamage) <= 0;

            return new DamageResult
            {
                finalDamage = finalDamage,
                rawDamage = rawDamage,
                modifierCount = modCount,
                targetDied = targetDied,
                debugInfo = $"{input.baseDamage:F0} → {rawDamage:F1} → {finalDamage} [{modCount} mods] {{{input.damageType}}}"
            };
        }

        /// <summary>
        /// 简化版——不经过 Modifier Chain，直接返回原始伤害。
        /// 用于没有 QueryBus handler 注册时的快速路径。
        /// </summary>
        public static DamageResult CalculateSimple(float baseDamage, float targetCurrentHealth)
        {
            int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage));
            damage = Mathf.Min(damage, Mathf.RoundToInt(targetCurrentHealth));

            return new DamageResult
            {
                finalDamage = damage,
                rawDamage = baseDamage,
                modifierCount = 0,
                targetDied = (targetCurrentHealth - damage) <= 0,
                debugInfo = $"Simple: {baseDamage:F0} → {damage}"
            };
        }
    }
}
