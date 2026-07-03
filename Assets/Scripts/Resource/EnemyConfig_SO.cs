using UnityEngine;

/// <summary>
/// 敌人全部可调参数。
/// 每种敌人类型可以创建自己的 SO 实例。
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Enemy Config")]
public class EnemyConfig_SO : ScriptableObject
{
    [Header("移动")]
    public float normalSpeed = 2f;
    public float chaseSpeedMultiplier = 1.5f;

    [Header("巡逻")]
    public float waitTime = 1.5f;
    public float lostTargetTime = 3f;  // 丢失目标后继续追击的时间

    [Header("攻击")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("检测器")]
    public Vector2 detectionCenterOffset = Vector2.zero;
    public Vector2 detectionBoxSize = new Vector2(1f, 1f);
    public float detectionDistance = 5f;

    [Header("掉落")]
    public int coinDropAmount = 3;
    public float coinSpawnRadius = 1.5f;
    public float coinSpawnRandomness = 0.3f;

    private void OnValidate()
    {
        normalSpeed = Mathf.Max(0.1f, normalSpeed);
        chaseSpeedMultiplier = Mathf.Max(1f, chaseSpeedMultiplier);
        attackRange = Mathf.Max(0.1f, attackRange);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);
        detectionDistance = Mathf.Max(0.1f, detectionDistance);
        coinDropAmount = Mathf.Max(0, coinDropAmount);
    }
}

/// <summary>
/// 角色生命/蓝量配置——玩家和 Boss 共用。
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Character Stats")]
public class CharacterStats_SO : ScriptableObject
{
    [Header("生命值")]
    public float maxHealth = 100f;

    [Header("蓝量")]
    public float maxMana = 100f;
    public float manaRegenRate = 5f;   // 每秒回蓝

    [Header("受击无敌")]
    public float invincibleAfterHitTime = 1.5f;  // 受击后无敌时间

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        maxMana = Mathf.Max(0, maxMana);
        manaRegenRate = Mathf.Max(0, manaRegenRate);
        invincibleAfterHitTime = Mathf.Max(0, invincibleAfterHitTime);
    }
}
