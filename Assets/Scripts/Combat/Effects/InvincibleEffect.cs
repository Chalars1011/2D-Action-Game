using UnityEngine;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 无敌 Effect——宿主免疫所有伤害。
    /// 通过 IQueryHandler 在伤害计算的防御方抗性阶段返回 0。
    /// 需要调用 BindTarget() 绑定宿主，才能正确过滤伤害查询。
    /// 工厂方法 Create() 已完成绑定。
    /// </summary>
    public class InvincibleEffect : IEffect, IQueryHandler<DamageModQuery, float>
    {
        public EffectId Id { get; }
        public StackRule StackRule => StackRule.RefreshDuration;
        public int MaxStacks => 1;
        public int CurrentStacks { get; set; } = 1;
        public float Duration { get; }
        public float RemainingTime { get; set; }

        private int _boundTargetId;

        public InvincibleEffect(float duration, string sourceType = "Item")
        {
            Id = new EffectId("Invincible", sourceType);
            Duration = duration;
            RemainingTime = duration;
        }

        public void BindTarget(int targetId) => _boundTargetId = targetId;

        public void OnApply(IEffectTarget target)
        {
            QueryBus.Register<DamageModQuery, float>(this);
        }

        public void OnTick(IEffectTarget target, float deltaTime) { }

        public void OnRemove(IEffectTarget target)
        {
            QueryBus.Unregister<DamageModQuery, float>(this);
        }

        public bool TryMergeWith(IEffect existing)
        {
            existing.RemainingTime = Mathf.Max(existing.RemainingTime, Duration);
            return true;
        }

        public float GetEffectStrength() => Duration;

        public float Handle(DamageModQuery query)
        {
            if (query.phase == ModPhase.DefenderResist &&
                query.targetInstanceId == _boundTargetId)
                return 0f;
            return 1f;
        }

        public static InvincibleEffect Create(float duration, IEffectTarget target)
        {
            var e = new InvincibleEffect(duration);
            e.BindTarget(target.InstanceId);
            return e;
        }
    }
}
