using System.Collections.Generic;
using UnityEngine;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 命中追踪器——防止同一次攻击多次命中同一目标。
    /// 
    /// 当前问题：AttackBase 的 OnTriggerEnter2D 可能在多帧内
    /// 反复触发（碰撞体停留），导致一刀打出多次伤害。
    /// 
    /// 解决方案：每次攻击生成唯一 ID，记录 (attackID, targetID)，
    /// 已命中的目标不会再被同一攻击伤害。
    /// 
    /// 用法：
    ///   在 AttackBase.OnEnable 中调用 HitTracker.BeginAttack()
    ///   在 AttackBase.OnTriggerEnter2D 中调用 HitTracker.TryHit()
    ///   只有 TryHit 返回 true 时才执行伤害逻辑
    /// </summary>
    public static class HitTracker
    {
        // (attackID, targetInstanceID) → 是否已命中
        private static readonly HashSet<(int, int)> _hitRegistry = new HashSet<(int, int)>();

        // 当前正在进行的攻击 ID 列表（用于清理）
        private static readonly List<int> _activeAttacks = new List<int>();

        private static int _nextAttackId = 1;

        /// <summary>
        /// 开始一次新攻击，返回唯一攻击 ID。
        /// 在攻击动画开始时调用（OnEnable / AnimationEvent）。
        /// </summary>
        public static int BeginAttack()
        {
            int id = _nextAttackId++;
            _activeAttacks.Add(id);
            return id;
        }

        /// <summary>
        /// 尝试命中目标。
        /// 返回 true = 可以造成伤害（首次命中此目标）
        /// 返回 false = 已经命中过此目标（跳过）
        /// </summary>
        public static bool TryHit(int attackId, int targetInstanceId)
        {
            var key = (attackId, targetInstanceId);
            if (_hitRegistry.Contains(key))
                return false;

            _hitRegistry.Add(key);
            return true;
        }

        /// <summary>
        /// 结束一次攻击，清理其所有命中记录。
        /// 在攻击动画结束时调用（OnDisable / AnimationEvent）。
        /// </summary>
        public static void EndAttack(int attackId)
        {
            // 移除所有与此 attackId 相关的命中记录
            var toRemove = new List<(int, int)>();
            foreach (var key in _hitRegistry)
            {
                if (key.Item1 == attackId)
                    toRemove.Add(key);
            }
            foreach (var key in toRemove)
                _hitRegistry.Remove(key);

            _activeAttacks.Remove(attackId);
        }

        /// <summary>
        /// 清空所有记录。场景切换时调用。
        /// </summary>
        public static void ClearAll()
        {
            _hitRegistry.Clear();
            _activeAttacks.Clear();
            _nextAttackId = 1;
        }

        /// <summary>
        /// 调试：当前活跃的攻击数 + 命中记录数。
        /// </summary>
        public static string GetDebugInfo()
        {
            return $"Attacks: {_activeAttacks.Count}, Hits recorded: {_hitRegistry.Count}";
        }
    }
}
