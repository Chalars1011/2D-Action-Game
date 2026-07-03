using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameArchitecture.Core;
using GameArchitecture.Combat;

/// <summary>
/// 【旧架构】玩家和敌人共用的血量/蓝量/伤害组件。
///
/// ⚠️ 此组件正在逐步废弃，新代码请使用：
///   - HealthComponent   → 纯血量/受伤/死亡（人人都有）
///   - PlayerStats       → 蓝量/持久化（仅玩家）
///   - EnemyDeathHandler → 掉落/死亡动画（仅敌人）
///
/// 当前此组件仍保留以确保旧预制体兼容。
/// 预制体迁移工具：Tools → Framework Migration → ...
/// </summary>
public class Character : MonoBehaviour, IEffectTarget
{

    public float currentHealth;
    public float maxHealth;
    public Transform BloodTransform;

    [Header("无敌时间")]
    public float WuDiJiShiQi;
    public float WuDiTime;
    public bool WuDiState = false;
    [HideInInspector] public bool isInHurtAnimation;
    [HideInInspector] public bool isHeavyHurt;

    [Header("蓝量系统")]
    public float currentMana = 100f;
    public float maxMana = 100f;
    public float manaRegenRate = 5f;

    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;
    public UnityEvent<Character> OnHealthChange;
    public UnityEvent<float> OnManaChange;

    private bool isPlayer = false;

    [Header("血量数据（SO持久化，拖PlayerHealthData进来）")]
    public PlayerHealthData healthData;

    [Header("=== 架构 v2：属性配置（可选，覆盖 Inspector 值）===")]
    [SerializeField] private CharacterStats_SO statsConfig;

    // === 架构 v2：Effect 管理器 ===
    [HideInInspector] public EffectManager effectManager;

    [Header("测试用")]
    public bool startWithCustomHealth;
    public float customStartHealth = 50f;

    private void ApplyStatsConfig()
    {
        if (statsConfig == null) return;
        maxHealth = statsConfig.maxHealth;
        maxMana = statsConfig.maxMana;
        manaRegenRate = statsConfig.manaRegenRate;
        WuDiTime = statsConfig.invincibleAfterHitTime;
    }

    private void Awake()
    {
        isPlayer = gameObject.CompareTag("Player");
        effectManager = new EffectManager(this);

        // === 架构 v2：从 SO 加载属性 ===
        ApplyStatsConfig();

        currentMana = maxMana;
        WuDiJiShiQi = WuDiTime;

        if (isPlayer && healthData != null)
        {
            if (startWithCustomHealth)
                currentHealth = customStartHealth;
            else
                currentHealth = healthData.currentHealth > 0 ? healthData.currentHealth : healthData.maxHealth;

            currentHealth = Mathf.Clamp(currentHealth, 0, healthData.maxHealth);
            maxHealth = healthData.maxHealth;
        }
        else if (isPlayer)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        OnHealthChange?.Invoke(this);
        OnManaChange?.Invoke(currentMana / maxMana);
    }

    void Start()
    {

    }

    void Update()
    {
        if (WuDiState)
        {
            WuDiJiShiQi -= Time.deltaTime;
            if (WuDiJiShiQi <= 0)
            {
                WuDiState = false;
            }
        }

        // 蓝量回复
        if (isPlayer && currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            OnManaChange?.Invoke(currentMana / maxMana);
        }

        // === 架构 v2：Effect Tick ===
        effectManager.Tick(Time.deltaTime);
    }

    // ============================================================
    // IEffectTarget 实现
    // ============================================================

    int IEffectTarget.InstanceId => GetInstanceID();

    void IEffectTarget.TakeDirectDamage(float amount, DamageType type)
    {
        if (WuDiState) return;
        currentHealth -= amount;
        OnHealthChange?.Invoke(this);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDie?.Invoke();
        }
        SaveHealthData();
    }

    void IEffectTarget.HealDirect(float amount)
    {
        HealHealth(amount);
    }

    private void LoadHealthData()
    {
        if (healthData != null && healthData.currentHealth > 0)
            currentHealth = healthData.currentHealth;
    }

    private void SaveHealthData()
    {
        if (isPlayer && healthData != null)
            healthData.currentHealth = currentHealth;
    }

    public void HealHealth(float healAmount)
    {
        if (healAmount <= 0) return;

        float newHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
        float actualHeal = newHealth - currentHealth;

        if (actualHeal > 0)
        {
            currentHealth = newHealth;
            OnHealthChange?.Invoke(this);
            SaveHealthData();
        }
    }

    public void ApplyHealEffect(int healAmount)
    {
        HealHealth(healAmount);
    }

    public bool ConsumeMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            OnManaChange?.Invoke(currentMana / maxMana);
            return true;
        }
        return false;
    }

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        OnManaChange?.Invoke(currentMana / maxMana);
    }

    // ============================================================
    // 架构 v2：伤害系统
    // ============================================================

    /// <summary>
    /// 通用伤害处理——所有伤害入口统一走这里。
    /// 构建 DamageInput → Calculator 计算 → 应用结果 → 发射事件
    /// 返回 true = 目标死亡
    /// </summary>
    private bool ApplyDamage(DamageInput input)
    {
        if (WuDiState) return false;

        isHeavyHurt = input.isHeavyHit || isInHurtAnimation;

        // === 走伤害计算链 ===
        var result = DamageCalculator.Calculate(input, currentHealth, maxHealth);
        currentHealth -= result.finalDamage;

        OnHealthChange?.Invoke(this);

        // === 死亡判断 ===
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDie?.Invoke();

            if (isPlayer)
            {
                EventBus.Emit(new PlayerDiedEvent { deathPosition = transform.position });
            }
            else
            {
                EventBus.Emit(new EnemyDeathEvent
                {
                    enemyInstanceId = GetInstanceID(),
                    enemyType = gameObject.name,
                    deathPosition = transform.position,
                    killedByPlayerInstanceId = input.attackerInstanceId
                });
            }

            if (isHeavyHurt) WuDi();
            SaveHealthData();
            return true;
        }

        // 非致命：发射受伤事件
        if (isPlayer)
        {
            EventBus.Emit(new PlayerDamagedEvent
            {
                damageTaken = result.finalDamage,
                currentHealth = currentHealth,
                maxHealth = maxHealth,
                attackerPosition = Vector3.zero,
                isHeavyHit = input.isHeavyHit
            });
        }

        if (isHeavyHurt) WuDi();
        SaveHealthData();
        return false;
    }

    public void TakeDmage(AttackBase attacker)
    {
        var input = DamageInput.FromAttack(attacker, this);
        bool died = ApplyDamage(input);
        if (!died)
            OnTakeDamage?.Invoke(attacker.transform);
    }

    public void TakeDmageTouch(EnemyCollisionDamage enemyCollision)
    {
        var input = DamageInput.FromCollision(enemyCollision, this);
        bool died = ApplyDamage(input);
        if (!died)
            OnTakeDamage?.Invoke(enemyCollision.transform);
    }

    public void WuDi()
    {
        if (!WuDiState)
        {
            WuDiState = true;
            WuDiJiShiQi = WuDiTime;
        }
    }
}
