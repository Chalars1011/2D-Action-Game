using UnityEngine;

namespace GameArchitecture.Core
{
    // ============================================================
    // 战斗事件
    // ============================================================

    /// <summary>
    /// 敌人死亡事件。由敌人的 Die 逻辑发射。
    /// 任务系统、掉落系统、统计系统等通过订阅此事件来响应。
    /// </summary>
    public struct EnemyDeathEvent
    {
        public int enemyInstanceId;           // 敌人实例 ID
        public string enemyType;              // 敌人类型标识
        public Vector3 deathPosition;         // 死亡位置（用于生成掉落物）
        public int killedByPlayerInstanceId;  // 击杀者的实例 ID（玩家=Player的GetInstanceID）
    }

    /// <summary>
    /// 玩家受伤事件。
    /// </summary>
    public struct PlayerDamagedEvent
    {
        public float damageTaken;
        public float currentHealth;
        public float maxHealth;
        public Vector3 attackerPosition;
        public bool isHeavyHit;
    }

    /// <summary>
    /// 玩家死亡事件。
    /// </summary>
    public struct PlayerDiedEvent
    {
        public Vector3 deathPosition;
    }

    /// <summary>
    /// 玩家复活事件。
    /// </summary>
    public struct PlayerRespawnedEvent
    {
        public Vector3 respawnPosition;
    }

    /// <summary>
    /// 命中检测事件。由判定系统发射，伤害系统订阅。
    /// </summary>
    public struct HitDetectedEvent
    {
        public int attackerInstanceId;
        public int targetInstanceId;
        public float baseDamage;
        public Vector3 contactPoint;
        public Vector3 knockbackDirection;
    }

    // ============================================================
    // 物品/经济事件
    // ============================================================

    /// <summary>
    /// 物品被拾取事件。
    /// </summary>
    public struct ItemPickedUpEvent
    {
        public string itemName;
        public int amount;
        public Vector3 pickupPosition;
    }

    /// <summary>
    /// 金币数量变化事件。
    /// </summary>
    public struct CurrencyChangedEvent
    {
        public string currencyName;
        public int newAmount;
        public int delta;
    }

    /// <summary>
    /// 血瓶数量变化事件。
    /// </summary>
    public struct BottleCountChangedEvent
    {
        public string bottleName;
        public int newAmount;
        public int delta;
    }

    /// <summary>
    /// 道具被使用事件。
    /// </summary>
    public struct ItemUsedEvent
    {
        public string itemName;
        public int remainingAmount;
    }

    // ============================================================
    // 世界/场景事件
    // ============================================================

    /// <summary>
    /// 场景加载完成事件。
    /// </summary>
    public struct SceneLoadedEvent
    {
        public string sceneName;
    }

    /// <summary>
    /// 传送门触发事件。
    /// </summary>
    public struct PortalTriggeredEvent
    {
        public string targetScene;
        public string spawnPointName;
        public int faceDirection;
    }

    // ============================================================
    // Boss 事件
    // ============================================================

    /// <summary>
    /// Boss 阶段变化事件。
    /// </summary>
    public struct BossPhaseChangedEvent
    {
        public string bossName;
        public int newPhase;
    }

    /// <summary>
    /// Boss 被击败事件。
    /// </summary>
    public struct BossDefeatedEvent
    {
        public string bossName;
    }

    // ============================================================
    // 游戏状态事件
    // ============================================================

    /// <summary>
    /// 游戏阶段变化事件。
    /// </summary>
    public struct GamePhaseChangedEvent
    {
        public GamePhase oldPhase;
        public GamePhase newPhase;
    }

    /// <summary>
    /// 游戏结束事件（通关或死亡选退出时）。
    /// </summary>
    public struct GameEndEvent
    {
        public bool isVictory;
    }

    // ============================================================
    // 黑板的枚举定义
    // ============================================================

    /// <summary>
    /// 游戏阶段——几乎所有系统都需要知道当前阶段。
    /// </summary>
    public enum GamePhase
    {
        Normal,     // 正常游戏
        Paused,     // 暂停菜单
        Cutscene,   // 过场动画
        Dialogue,   // 对话中
        Loading,    // 加载中
        Dead        // 死亡中（播放死亡动画/过渡）
    }
}
