using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameArchitecture.Narrative
{
    /// <summary>
    /// 游戏快照——当前状态的完整切片。
    /// 存档时先拍快照，命令序列只保留快照之后的新事件。
    /// 
    /// 混合模式：
    ///   快照 → 大量数据一次存（玩家位置、血量、金币、敌人状态...）
    ///   命令 → 关键事件精确追溯（门开了、Boss死了、任务进度...）
    /// 
    /// 加载 = 恢复快照 + 重放快照之后的命令
    /// </summary>
    [Serializable]
    public class GameSnapshot
    {
        // ---- 玩家状态 ----
        public float playerPosX, playerPosY, playerPosZ;
        public float playerHealth, playerMaxHealth;
        public float playerMana, playerMaxMana;

        // ---- 资源 ----
        public int goldAmount;
        public int bloodBottleAmount;

        // ---- 道具数量 ----
        public Dictionary<string, int> itemAmounts = new();

        // ---- 标志位 ----
        public Dictionary<string, bool> flags = new();

        // ---- 场景 ----
        public string sceneName;

        // ---- 敌人生存状态 ----
        public List<int> deadEnemyIds = new();

        /// <summary>从当前游戏状态拍快照。</summary>
        public static GameSnapshot Capture()
        {
            var snap = new GameSnapshot();
            snap.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 玩家
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                snap.playerPosX = player.transform.position.x;
                snap.playerPosY = player.transform.position.y;
                snap.playerPosZ = player.transform.position.z;

                var c = player.GetComponent<Character>();
                if (c != null)
                {
                    snap.playerHealth = c.currentHealth;
                    snap.playerMaxHealth = c.maxHealth;
                    snap.playerMana = c.currentMana;
                    snap.playerMaxMana = c.maxMana;
                }
            }

            // 资源
            snap.goldAmount = PlayerItemManager.Instance != null
                ? PlayerItemManager.Instance.GetCurrencyAmount("Gold") : 0;
            snap.bloodBottleAmount = PlayerItemManager.Instance != null
                ? PlayerItemManager.Instance.GetBottleAmount("BloodBottle") : 0;

            // 标志位
            snap.flags = GameState.ExportFlags();

            return snap;
        }

        /// <summary>把快照恢复到游戏状态。</summary>
        public void Restore()
        {
            // 场景由 SaveManager 处理

            // 玩家
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 位置
                var ctrl = player.GetComponent<PlayerController>();
                if (ctrl != null)
                {
                    ctrl.transform.position = new Vector3(playerPosX, playerPosY, playerPosZ);
                    // 关闭输入避免移动
                    ctrl.CloseInputController();
                    ctrl.OpenInputController();
                }

                // 血量
                var c = player.GetComponent<Character>();
                if (c != null)
                {
                    c.currentHealth = playerHealth;
                    c.maxHealth = playerMaxHealth;
                    c.currentMana = playerMana;
                    c.maxMana = playerMaxMana;
                }
            }

            // 资源（先清零再设置）
            if (PlayerItemManager.Instance != null)
            {
                int currentGold = PlayerItemManager.Instance.GetCurrencyAmount("Gold");
                if (currentGold > 0) PlayerItemManager.Instance.SpendCurrency("Gold", currentGold);
                if (goldAmount > 0) PlayerItemManager.Instance.AddCurrency("Gold", goldAmount);
            }

            // 标志位
            GameState.RestoreFlags(flags);
        }
    }
}
