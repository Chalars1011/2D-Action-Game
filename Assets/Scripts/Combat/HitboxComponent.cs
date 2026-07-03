using UnityEngine;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 纯判定组件——检测碰撞、造成伤害。不管音效、震屏、特效。
    /// 
    /// 和旧 AttackBase 的区别：
    ///   AttackBase: 判定 + 音效 + 震屏 + 冻结 + 血特效（全部耦合）
    ///   HitboxComponent: 只做判定和伤害，表现层通过事件独立订阅
    /// 
    /// 用法：
    ///   挂到攻击特效预制体上。伤害值从外部注入（技能/Buff/装备计算后传入）。
    /// 
    /// 事件流:
    ///   OnTriggerEnter2D
    ///     → HitTracker.TryHit (去重)
    ///     → target.HealthComponent.TakeDamage (伤害计算链)
    ///     → EventBus.Emit(HitLandedEvent) (表现层订阅)
    /// </summary>
    public class HitboxComponent : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private int _baseDamage = 10;
        [SerializeField] private DamageType _damageType = DamageType.Physical;
        [SerializeField] private bool _isHeavyHit = false;
        [SerializeField] private Vector2 _knockbackForce = new Vector2(5f, 2f);

        [Header("Target")]
        [SerializeField] private LayerMask _targetLayers = -1; // 默认所有层
        [Tooltip("不伤害这些层上的对象")]
        [SerializeField] private LayerMask _ignoreLayers;

        [Header("Behavior")]
        [Tooltip("命中后是否失活（大多数攻击为true）")]
        [SerializeField] private bool _deactivateOnHit = true;
        [Tooltip("失活前延迟（秒），0=立即失活")]
        [SerializeField] private float _deactivateDelay = 0f;

        private bool _canHit = true;
        private int _attackId;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void OnEnable()
        {
            _canHit = true;
            _attackId = HitTracker.BeginAttack();
        }

        private void OnDisable()
        {
            HitTracker.EndAttack(_attackId);
        }

        // ============================================================
        // Public API — 外部注入伤害值
        // ============================================================

        /// <summary>
        /// 设置本次攻击的伤害值。由技能系统/装备系统在生成攻击时调用。
        /// </summary>
        public void SetDamage(int damage)
        {
            _baseDamage = damage;
        }

        public void SetDamageType(DamageType type)
        {
            _damageType = type;
        }

        public void SetHeavyHit(bool heavy)
        {
            _isHeavyHit = heavy;
        }

        public void SetKnockback(Vector2 force)
        {
            _knockbackForce = force;
        }

        // ============================================================
        // Hit Detection
        // ============================================================

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_canHit) return;
            if (!IsValidTarget(other)) return;

            int targetId = other.GetInstanceID();
            if (!HitTracker.TryHit(_attackId, targetId))
                return;

            // 获取目标的 HealthComponent
            var health = other.GetComponent<HealthComponent>();
            if (health == null || health.IsDead) return;

            // 构建伤害输入——击退方向在实际命中时计算
            Vector2 dirToTarget = ((Vector2)(health.transform.position - transform.position)).normalized;
            Vector2 knockback = new Vector2(
                dirToTarget.x * _knockbackForce.x,
                _knockbackForce.y); // Y 分量用配置值（向上击飞）

            var input = new DamageInput
            {
                baseDamage = _baseDamage,
                damageType = _damageType,
                attackerInstanceId = GetInstanceID(),
                targetInstanceId = health.GetInstanceID(),
                isHeavyHit = _isHeavyHit,
                knockbackDirection = knockback
            };

            // 造成伤害
            int actualDamage = health.TakeDamage(input);

            // 广播命中事件——表现层订阅此事件做震屏/音效/特效
            EventBus.Emit(new HitLandedEvent
            {
                attackerId = GetInstanceID(),
                targetId = health.GetInstanceID(),
                contactPoint = other.ClosestPoint(transform.position),
                damageAmount = actualDamage,
                damageType = _damageType,
                isHeavy = _isHeavyHit,
                isFatal = health.IsDead
            });

            if (_deactivateOnHit)
            {
                if (_deactivateDelay > 0)
                    Invoke(nameof(Deactivate), _deactivateDelay);
                else
                    Deactivate();
            }
        }

        private void Deactivate()
        {
            _canHit = false;
            gameObject.SetActive(false);
        }

        private bool IsValidTarget(Collider2D other)
        {
            // 层过滤
            int otherLayer = 1 << other.gameObject.layer;
            if ((_ignoreLayers.value & otherLayer) != 0) return false;
            if (_targetLayers.value != -1 && (_targetLayers.value & otherLayer) == 0) return false;

            // 必须有 HealthComponent
            return other.GetComponent<HealthComponent>() != null;
        }
    }

    // ============================================================
    // Events
    // ============================================================

    /// <summary>
    /// 攻击命中事件——表现层（震屏/音效/特效）订阅此事件。
    /// HitboxComponent 不直接调用 AudioManager 或 EffectPoolManager。
    /// </summary>
    public struct HitLandedEvent
    {
        public int attackerId;
        public int targetId;
        public Vector2 contactPoint;
        public int damageAmount;
        public DamageType damageType;
        public bool isHeavy;
        public bool isFatal;
    }
}
