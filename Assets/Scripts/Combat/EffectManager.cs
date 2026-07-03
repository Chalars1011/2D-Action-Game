using System.Collections.Generic;
using UnityEngine;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// EffectManager —— 单个实体（玩家或敌人）身上所有 Effect 的管理器。
    /// 每个拥有 Character 组件的实体创建自己的 EffectManager 实例。
    /// 
    /// 为什么不在全局做？
    /// Effect 的生命周期绑定宿主。宿主死亡 → 所有 Effect 移除 → OnRemove 自动调用。
    /// 全局管理需要手动维护"谁拥有哪个 Effect"的映射。
    /// </summary>
    public class EffectManager
    {
        private readonly IEffectTarget _owner;
        private readonly List<IEffect> _activeEffects = new List<IEffect>();
        private readonly List<IEffect> _pendingRemoval = new List<IEffect>();

        public int ActiveEffectCount => _activeEffects.Count;

        public EffectManager(IEffectTarget owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// 添加一个 Effect。
        /// 流程：查同名 → 合并规则 → 失败则新增 → 调 OnApply
        /// </summary>
        public void Add(IEffect effect)
        {
            var existing = FindById(effect.Id);

            if (existing != null)
            {
                // 尝试让已有 Effect 自己处理合并
                if (existing.TryMergeWith(effect))
                    return;

                // 按叠加规则处理
                switch (effect.StackRule)
                {
                    case StackRule.Independent:
                        if (existing.CurrentStacks < existing.MaxStacks)
                        {
                            existing.CurrentStacks++;
                            EventBus.Emit(new EffectStackChangedEvent
                            {
                                targetId = _owner.InstanceId,
                                effectName = effect.Id.Name,
                                newStacks = existing.CurrentStacks
                            });
                            return;
                        }
                        return; // 满层忽略

                    case StackRule.RefreshDuration:
                        existing.RemainingTime = effect.Duration;
                        return;

                    case StackRule.StackIntensity:
                        if (existing.CurrentStacks < existing.MaxStacks)
                        {
                            existing.CurrentStacks++;
                            existing.RemainingTime = effect.Duration;
                            existing.OnApply(_owner); // 重新触发 OnApply（刷新强度）
                            EventBus.Emit(new EffectStackChangedEvent
                            {
                                targetId = _owner.InstanceId,
                                effectName = effect.Id.Name,
                                newStacks = existing.CurrentStacks
                            });
                            return;
                        }
                        return;

                    case StackRule.Replace:
                        Remove(existing);
                        break;

                    case StackRule.StrongestFirst:
                        if (effect.GetEffectStrength() <= existing.GetEffectStrength())
                            return;
                        Remove(existing);
                        break;
                }
            }

            // 新增 Effect
            _activeEffects.Add(effect);
            effect.OnApply(_owner);

            EventBus.Emit(new EffectAppliedEvent
            {
                targetId = _owner.InstanceId,
                effectName = effect.Id.Name,
                remainingTime = effect.Duration
            });
        }

        /// <summary>
        /// 移除一个 Effect。无论自然过期还是强制移除都走这里。
        /// OnRemove 保证被调用。
        /// </summary>
        public void Remove(IEffect effect)
        {
            effect.OnRemove(_owner);
            _activeEffects.Remove(effect);

            EventBus.Emit(new EffectRemovedEvent
            {
                targetId = _owner.InstanceId,
                effectName = effect.Id.Name
            });
        }

        /// <summary>
        /// 移除所有 Effect（宿主死亡时调用）。
        /// </summary>
        public void ClearAll()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
                Remove(_activeEffects[i]);
        }

        /// <summary>
        /// 每帧更新所有 Effect。
        /// 先收集待移除列表再遍历移除，避免迭代中修改列表。
        /// </summary>
        public void Tick(float deltaTime)
        {
            _pendingRemoval.Clear();

            foreach (var effect in _activeEffects)
            {
                effect.OnTick(_owner, deltaTime);

                if (effect.Duration > 0) // -1 = 无限
                {
                    effect.RemainingTime -= deltaTime;
                    if (effect.RemainingTime <= 0)
                        _pendingRemoval.Add(effect);
                }
            }

            foreach (var effect in _pendingRemoval)
                Remove(effect);
        }

        /// <summary>
        /// 检查是否有指定名称的 Effect。
        /// </summary>
        public bool HasEffect(string effectName)
        {
            foreach (var e in _activeEffects)
                if (e.Id.Name == effectName) return true;
            return false;
        }

        private IEffect FindById(EffectId id)
        {
            foreach (var e in _activeEffects)
                if (e.Id.Equals(id)) return e;
            return null;
        }
    }

    // ============================================================
    // Effect 系统的事件定义
    // ============================================================

    public struct EffectAppliedEvent
    {
        public int targetId;
        public string effectName;
        public float remainingTime;
    }

    public struct EffectRemovedEvent
    {
        public int targetId;
        public string effectName;
    }

    public struct EffectStackChangedEvent
    {
        public int targetId;
        public string effectName;
        public int newStacks;
    }
}
