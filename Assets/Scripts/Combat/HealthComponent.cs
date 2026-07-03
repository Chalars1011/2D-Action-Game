using UnityEngine;
using UnityEngine.Events;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 纯血量组件——管受伤、治疗、死亡。不管"你是谁"。
    /// 
    /// 设计原则：
    /// 1. 零玩家/敌人判断逻辑——不出现 if(isPlayer)
    /// 2. 所有外部响应通过 UnityEvent + EventBus 事件
    /// 3. 可独立测试——不依赖 PlayerController 或 EnemyBase
    /// 
    /// 用法：
    ///   挂到任何需要"有血量、会受伤、会死"的 GameObject 上。
    ///   玩家 → 额外挂 PlayerStats 组件处理蓝量/存档
    ///   敌人 → 额外挂 EnemyDeathHandler 组件处理掉落/死亡动画
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [Header("Basic")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;

        [Header("Invincibility")]
        [SerializeField] private float _invincibleAfterHit = 1.5f;
        private float _invincibleTimer;
        private bool _isInvincible;

        [Header("Events")]
        public UnityEvent<DamageInfo> OnDamaged;     // 受伤（非致命）
        public UnityEvent<DamageInfo> OnFatalHit;     // 致命一击
        public UnityEvent OnDied;                     // 死亡
        public UnityEvent<float, float> OnHealthChanged; // (current, max)
        public UnityEvent<bool> OnInvincibleChanged;  // 无敌状态变化

        // ============================================================
        // Properties
        // ============================================================

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0;
        public bool IsDead => _currentHealth <= 0;
        public bool IsInvincible => _isInvincible;
        public bool IsAlive => _currentHealth > 0;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            if (_currentHealth <= 0)
                _currentHealth = _maxHealth;

            // 注册到 HitPresentation 缓存
            HitPresentation.RegisterTarget(GetInstanceID(), transform);
        }

        private void OnDestroy()
        {
            HitPresentation.UnregisterTarget(GetInstanceID());
        }

        private void Update()
        {
            if (_isInvincible)
            {
                _invincibleTimer -= Time.deltaTime;
                if (_invincibleTimer <= 0)
                {
                    _isInvincible = false;
                    OnInvincibleChanged?.Invoke(false);
                }
            }
        }

        // ============================================================
        // Configuration
        // ============================================================

        public void SetMaxHealth(float max)
        {
            _maxHealth = Mathf.Max(1, max);
            if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void SetCurrentHealth(float amount)
        {
            _currentHealth = Mathf.Clamp(amount, 0, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            if (_currentHealth <= 0)
                OnDied?.Invoke();
        }

        public void SetInvincibleDuration(float duration)
        {
            _invincibleAfterHit = Mathf.Max(0, duration);
        }

        // ============================================================
        // Damage
        // ============================================================

        /// <summary>
        /// 造成伤害——主入口。
        /// 内部走 DamageCalculator 的 Modifier Chain。
        /// </summary>
        /// <returns>实际造成的伤害值</returns>
        public int TakeDamage(DamageInput input)
        {
            if (_isInvincible || IsDead)
                return 0;

            var result = DamageCalculator.Calculate(input, _currentHealth, _maxHealth);
            _currentHealth -= result.finalDamage;

            var info = new DamageInfo
            {
                amount = result.finalDamage,
                rawAmount = result.rawDamage,
                damageType = input.damageType,
                sourceId = input.attackerInstanceId,
                isHeavy = input.isHeavyHit,
                modifierCount = result.modifierCount,
                isFatal = _currentHealth <= 0
            };

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                OnFatalHit?.Invoke(info);
                OnDied?.Invoke();

                // EventBus 广播（全局订阅）
                EventBus.Emit(new EntityDeathEvent
                {
                    entityId = GetInstanceID(),
                    entityName = gameObject.name,
                    deathPosition = transform.position,
                    killerId = input.attackerInstanceId
                });
            }
            else
            {
                OnDamaged?.Invoke(info);
                // 受击后短暂无敌
                if (_invincibleAfterHit > 0)
                    TriggerInvincible();
            }

            return result.finalDamage;
        }

        /// <summary>
        /// 直接伤害——绕过 DamageCalculator（用于 DOT、环境伤害）。
        /// </summary>
        public int TakeDirectDamage(float amount, DamageType type = DamageType.True)
        {
            return TakeDamage(new DamageInput
            {
                baseDamage = amount,
                damageType = type,
                attackerInstanceId = 0,
                targetInstanceId = GetInstanceID(),
                isHeavyHit = false,
                knockbackDirection = Vector2.zero
            });
        }

        // ============================================================
        // Healing
        // ============================================================

        public float Heal(float amount)
        {
            if (IsDead || amount <= 0) return 0;

            float before = _currentHealth;
            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
            float actual = _currentHealth - before;

            if (actual > 0)
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            return actual;
        }

        // ============================================================
        // Invincibility
        // ============================================================

        public void TriggerInvincible()
        {
            if (_invincibleAfterHit <= 0) return;
            _invincibleTimer = _invincibleAfterHit;
            _isInvincible = true;
            OnInvincibleChanged?.Invoke(true);
        }

        /// <summary>
        /// 外部设置无敌时间（如无敌道具）。
        /// duration <= 0 表示取消无敌。
        /// </summary>
        public void SetInvincibleByTime(float duration)
        {
            if (duration <= 0)
            {
                _isInvincible = false;
                _invincibleTimer = 0;
                OnInvincibleChanged?.Invoke(false);
                return;
            }
            _invincibleTimer = duration;
            _isInvincible = true;
            OnInvincibleChanged?.Invoke(true);
        }
    }

    // ============================================================
    // 轻量数据结构
    // ============================================================

    /// <summary>
    /// 伤害信息——携带这次受伤的完整上下文。
    /// 用于 UI 显示、战斗日志、回放系统。
    /// </summary>
    public struct DamageInfo
    {
        public int amount;          // 最终伤害
        public float rawAmount;     // 修正前伤害
        public DamageType damageType;
        public int sourceId;        // 伤害来源的 InstanceID
        public bool isHeavy;        // 是否重击
        public int modifierCount;   // 应用了多少修正器
        public bool isFatal;        // 是否致死
    }

    /// <summary>
    /// 实体死亡事件——广播给全局。
    /// </summary>
    public struct EntityDeathEvent
    {
        public int entityId;
        public string entityName;
        public Vector3 deathPosition;
        public int killerId;
    }

}
