using System.Collections.Generic;
using UnityEngine;

namespace GameArchitecture.Narrative
{
    /// <summary>
    /// 全局游戏状态——集中管理所有需要持久化的标志位和数据。
    /// 
    /// 作用：
    ///   1. SaveManager.Load 时调用 ResetToInitial() 清空所有状态
    ///   2. 命令重放时状态在这里被修改
    ///   3. 标志位（门开了、宝箱拿了、Boss 死了）集中存储
    /// 
    /// 当前实现用内存字典。存档加载时重建。
    /// </summary>
    public static class GameState
    {
        private static readonly Dictionary<string, bool> _flags = new();

        /// <summary>设置标志位（开门、拿宝箱、杀Boss等）。</summary>
        public static void SetFlag(string flagId, bool value)
        {
            _flags[flagId] = value;
            Debug.Log($"[GameState] Flag: {flagId} = {value}");
        }

        /// <summary>检查标志位。</summary>
        public static bool GetFlag(string flagId)
        {
            return _flags.TryGetValue(flagId, out bool v) && v;
        }

        /// <summary>是否有这个标志。</summary>
        public static bool HasFlag(string flagId) => _flags.ContainsKey(flagId);

        /// <summary>
        /// 重置到初始状态——Load 时调用。
        /// 清空所有标志，金币/血量由各自的命令重放来恢复。
        /// </summary>
        public static void ResetToInitial()
        {
            _flags.Clear();
            Debug.Log("[GameState] Reset to initial");
        }

        public static Dictionary<string, bool> ExportFlags()
        {
            return new Dictionary<string, bool>(_flags);
        }

        public static void RestoreFlags(Dictionary<string, bool> flags)
        {
            _flags.Clear();
            if (flags != null)
                foreach (var kv in flags) _flags[kv.Key] = kv.Value;
        }

        public static string Dump()
        {
            var lines = new List<string>();
            foreach (var kv in _flags)
                lines.Add($"{kv.Key}={kv.Value}");
            return string.Join(", ", lines);
        }
    }
}
