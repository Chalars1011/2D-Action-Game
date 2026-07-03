using UnityEngine;
using UnityEngine.Events;
using GameArchitecture.Combat;

namespace GameArchitecture.Actor
{
    /// <summary>
    /// 玩家专属属性——蓝量、存档持久化。
    /// 不包含血量逻辑（血量在 HealthComponent 里）。
    /// 
    /// 为什么独立？
    ///   敌人不需要蓝量、不需要存档持久化。
    ///   把蓝量放在通用的 Character 里 → 敌人也有了蓝量字段（浪费+混淆）。
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Mana")]
        [SerializeField] private float _maxMana = 100f;
        [SerializeField] private float _currentMana;
        [SerializeField] private float _manaRegenRate = 5f;

        [Header("Health Persistence")]
        [SerializeField] private PlayerHealthData _healthData;

        [Header("Config")]
        [SerializeField] private CharacterStats_SO _statsConfig;

        [Header("Events")]
        public UnityEvent<float> OnManaChanged; // manaPercent

        private HealthComponent _health;

        // ============================================================
        // Properties
        // ============================================================

        public float CurrentMana => _currentMana;
        public float MaxMana => _maxMana;
        public float ManaPercent => _maxMana > 0 ? _currentMana / _maxMana : 0;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();

            if (_statsConfig != null)
            {
                if (_health != null)
                {
                    _health.SetMaxHealth(_statsConfig.maxHealth);
                    _health.SetInvincibleDuration(_statsConfig.invincibleAfterHitTime);
                }
                _maxMana = _statsConfig.maxMana;
                _manaRegenRate = _statsConfig.manaRegenRate;
            }

            _currentMana = _maxMana;

            // 从持久化数据恢复血量
            if (_healthData != null && _healthData.currentHealth > 0 && _health != null)
                _health.SetCurrentHealth(_healthData.currentHealth);

            // 事件驱动持久化——血量变化时写 SO，不每帧轮询
            if (_health != null)
                _health.OnHealthChanged.AddListener(OnHealthChanged);
        }

        private void Update()
        {
            // 蓝量回复
            if (_currentMana < _maxMana)
            {
                _currentMana += _manaRegenRate * Time.deltaTime;
                _currentMana = Mathf.Clamp(_currentMana, 0, _maxMana);
                OnManaChanged?.Invoke(ManaPercent);
            }
        }

        /// <summary>
        /// 血量变化时才持久化——由 HealthComponent.OnHealthChanged 事件驱动。
        /// 不在 Update 里每帧写。
        /// </summary>
        private void OnHealthChanged(float current, float max)
        {
            if (_healthData != null)
                _healthData.currentHealth = current;
        }

        // ============================================================
        // Mana
        // ============================================================

        public bool ConsumeMana(float amount)
        {
            if (_currentMana >= amount)
            {
                _currentMana -= amount;
                OnManaChanged?.Invoke(ManaPercent);
                return true;
            }
            return false;
        }

        public void RestoreMana(float amount)
        {
            _currentMana = Mathf.Clamp(_currentMana + amount, 0, _maxMana);
            OnManaChanged?.Invoke(ManaPercent);
        }
    }
}
