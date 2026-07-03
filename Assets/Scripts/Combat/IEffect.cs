namespace GameArchitecture.Combat
{
    /// <summary>
    /// Effect 的唯一标识。同名同源 = 同类 Effect。
    /// 用于叠加规则判断和存档序列化。
    /// </summary>
    public struct EffectId
    {
        public string Name;       // "Invincible", "Poison", "AttackUp"
        public string SourceType; // "Skill", "Item", "Equipment", "Environment"

        public EffectId(string name, string sourceType = "Skill")
        {
            Name = name;
            SourceType = sourceType;
        }

        public override bool Equals(object obj)
        {
            if (obj is EffectId other)
                return Name == other.Name && SourceType == other.SourceType;
            return false;
        }

        public override int GetHashCode() => (Name, SourceType).GetHashCode();
        public override string ToString() => $"{Name}({SourceType})";
    }

    /// <summary>
    /// 叠加规则——同名 Effect 再次添加时的行为。
    /// </summary>
    public enum StackRule
    {
        Independent,       // 独立——每层创建新实例（中毒：每层独立跳伤害）
        RefreshDuration,   // 刷新——只重置计时器（加速 Buff：不能叠加层数）
        StackIntensity,    // 叠层——每层增强效果，有最大层数（流血：10层）
        Replace,           // 替换——新 Effect 完全取代旧的
        StrongestFirst     // 取强——新不如旧则忽略（护盾 Buff）
    }

    /// <summary>
    /// Effect 是"在一段时间内持续修改游戏状态"的抽象。
    /// 
    /// 所有 Buff、Debuff、光环、状态异常都实现这个接口。
    /// 为什么用接口而不是基类？
    /// 因为不同类型 Effect 差异极大——一个 DOT 和一个属性修改
    /// 几乎没有共同逻辑，强行用基类会导致基类膨胀。
    /// </summary>
    public interface IEffect
    {
        EffectId Id { get; }
        StackRule StackRule { get; }
        int MaxStacks { get; }          // 最大叠加层数（1 = 不叠加）
        int CurrentStacks { get; set; }
        float Duration { get; }         // -1 = 无限，0 = 瞬时
        float RemainingTime { get; set; }

        /// <summary>Effect 添加到目标时调用一次。</summary>
        void OnApply(IEffectTarget target);

        /// <summary>每帧调用（Duration > 0 时）。DOT/HOT 在这里实现。</summary>
        void OnTick(IEffectTarget target, float deltaTime);

        /// <summary>Effect 过期或被强制移除时调用。保证清理逻辑不遗漏。</summary>
        void OnRemove(IEffectTarget target);

        /// <summary>同名 Effect 尝试添加时的合并逻辑。true = 已处理，不需要新增。</summary>
        bool TryMergeWith(IEffect existing);

        /// <summary>获取效果强度（用于 StrongestFirst 比较）。</summary>
        float GetEffectStrength();
    }

    /// <summary>
    /// Effect 宿主接口——Character 实现此接口。
    /// </summary>
    public interface IEffectTarget
    {
        int InstanceId { get; }
        void TakeDirectDamage(float amount, DamageType type);
        void HealDirect(float amount);
    }
}
