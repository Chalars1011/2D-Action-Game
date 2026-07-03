using UnityEngine;

namespace GameArchitecture.Core
{
    /// <summary>
    /// 全局状态黑板。只存放真正全局的状态——全游戏不超过 15 个字段。
    /// 
    /// 严格规则：
    /// 1. 只有 River Manager 级别的代码能写 Blackboard
    /// 2. 不允许放任何业务数据（敌人血量、背包内容...）
    /// 3. 值变化必须通过 EventBus 通知（已在 Set 方法内自动发射事件）
    /// 4. 不允许在 Update 里轮询 Blackboard
    /// </summary>
    public static class Blackboard
    {
        // ============================================================
        // 游戏阶段
        // ============================================================
        public static GamePhase CurrentPhase { get; private set; } = GamePhase.Normal;

        public static void SetPhase(GamePhase phase)
        {
            var old = CurrentPhase;
            if (old == phase) return;
            CurrentPhase = phase;
            EventBus.Emit(new GamePhaseChangedEvent { oldPhase = old, newPhase = phase });
        }

        // ============================================================
        // 玩家引用（Transform —— 几乎所有系统都需要访问）
        // ============================================================
        public static Transform PlayerTransform { get; private set; }

        public static void SetPlayer(Transform player)
        {
            PlayerTransform = player;
        }

        // ============================================================
        // 时间缩放
        // ============================================================
        private static float _timeScale = 1f;
        public static float TimeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = Mathf.Max(0f, value);
                Time.timeScale = _timeScale;
            }
        }

        // ============================================================
        // 便捷属性
        // ============================================================
        public static bool IsPaused =>
            CurrentPhase == GamePhase.Paused ||
            CurrentPhase == GamePhase.Cutscene ||
            CurrentPhase == GamePhase.Dialogue;

        public static bool IsPlayerDead => CurrentPhase == GamePhase.Dead;

        public static bool CanPlayerAct =>
            CurrentPhase == GamePhase.Normal &&
            !IsPaused &&
            PlayerTransform != null;
    }
}
