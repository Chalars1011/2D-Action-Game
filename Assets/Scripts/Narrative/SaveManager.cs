using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameArchitecture.Narrative
{
    /// <summary>
    /// 存档管理器——混合模式存档。
    ///
    /// 快照层（IStateTracked）：各系统上报自己的状态，SaveManager 收集到一个字典。
    ///   加载时直接恢复字典 → O(1) 恢复，不怕命令序列太长。
    ///
    /// 命令层（IGameCommand）：关键事件记录为命令序列。
    ///   加载时在快照之上重放 → 精确追溯，支持回放/撤销。
    ///
    /// 用法：
    ///   SaveManager.Instance.Execute(new AddGoldCommand(100));
    ///   SaveManager.Instance.Save(1);
    ///   SaveManager.Instance.Load(1);
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int CURRENT_VERSION = 1;
        private const int MAX_COMMANDS_BEFORE_CHECKPOINT = 1000;

        private readonly List<IGameCommand> _commandHistory = new();
        private readonly Dictionary<string, IStateTracked> _trackers = new();
        private int _lastCheckpointIndex;

        public int CommandCount => _commandHistory.Count;

        /// <summary>注册持久化系统。系统在 Awake 时调用。</summary>
        public void RegisterTracker(IStateTracked tracker)
        {
            _trackers[tracker.StateId] = tracker;
        }

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else Destroy(gameObject);
        }

        // ============================================================
        // Execute & Record
        // ============================================================

        /// <summary>
        /// 执行命令并记录到历史。
        /// 所有持久状态修改必须通过这个方法——这是存档的唯一切入点。
        /// </summary>
        public void Execute(IGameCommand command)
        {
            command.Execute();
            _commandHistory.Add(command);
        }

        // ============================================================
        // Save
        // ============================================================

        /// <summary>保存到指定存档槽。</summary>
        public bool Save(int slot)
        {
            try
            {
                // 混合模式：收集所有系统的状态 + 命令序列
                var allState = new Dictionary<string, string>();
                foreach (var t in _trackers.Values)
                    allState[t.StateId] = t.CaptureState();

                var data = new SaveData
                {
                    version = CURRENT_VERSION,
                    timestamp = DateTime.Now.Ticks,
                    state = allState,
                    commands = new List<GameCommandData>()
                };

                // 只保存上次检查点之后的新命令
                for (int i = _lastCheckpointIndex; i < _commandHistory.Count; i++)
                    data.commands.Add(_commandHistory[i].Serialize());

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(GetPath(slot), json);
                Debug.Log($"[SaveManager] Saved slot {slot}: {data.commands.Count} commands");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
                return false;
            }
        }

        // ============================================================
        // Load
        // ============================================================

        /// <summary>从指定存档槽加载。</summary>
        public bool Load(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path)) return false;

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return false;

                // 恢复各系统的状态
                if (data.state != null)
                {
                    foreach (var kv in data.state)
                    {
                        if (_trackers.TryGetValue(kv.Key, out var t))
                            t.RestoreState(kv.Value);
                    }
                }

                // 清空命令历史
                _commandHistory.Clear();
                _lastCheckpointIndex = 0;

                // 重放命令（新命令为空时说明是完整快照加载）
                foreach (var cmd in data.commands)
                {
                    var command = CommandFactory.Deserialize(cmd);
                    if (command != null)
                    {
                        command.Execute();
                        _commandHistory.Add(command);
                    }
                }

                Debug.Log($"[SaveManager] Loaded slot {slot}: {_commandHistory.Count} commands replayed");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}");
                return false;
            }
        }

        // ============================================================
        // Checkpoint
        // ============================================================

        /// <summary>
        /// 创建检查点——重置命令历史起点。
        /// 防止 100 小时游戏后命令序列太长。
        /// 建议在进入新场景、Boss 战后调用。
        /// </summary>
        public void CreateCheckpoint()
        {
            _lastCheckpointIndex = _commandHistory.Count;
            Debug.Log($"[SaveManager] Checkpoint at command #{_lastCheckpointIndex}");
        }

        /// <summary>存档是否存在。</summary>
        public bool Exists(int slot) => File.Exists(GetPath(slot));

        /// <summary>删除存档。</summary>
        public void Delete(int slot)
        {
            if (File.Exists(GetPath(slot))) File.Delete(GetPath(slot));
        }

        // ============================================================
        // Helpers
        // ============================================================

        private static string GetPath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
        }

        public void AutoCheckpoint()
        {
            if (_commandHistory.Count - _lastCheckpointIndex >= MAX_COMMANDS_BEFORE_CHECKPOINT)
                CreateCheckpoint();
        }

        private void Update()
        {
            AutoCheckpoint();
        }
    }

    // ============================================================
    // Data Types
    // ============================================================

    [Serializable]
    public class SaveData
    {
        public int version;
        public long timestamp;
        public Dictionary<string, string> state;  // 各系统状态：systemId → json
        public List<GameCommandData> commands;     // 命令序列
    }
}
