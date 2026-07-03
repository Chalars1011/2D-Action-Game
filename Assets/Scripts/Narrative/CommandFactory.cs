using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameArchitecture.Narrative
{
    /// <summary>
    /// 命令工厂——根据类型名反序列化命令。
    /// 新增命令类型时在这里注册。
    /// </summary>
    public static class CommandFactory
    {
        private static readonly Dictionary<string, Func<string, IGameCommand>> _registry = new();

        static CommandFactory()
        {
            Register<AddGoldCommand>();
            Register<AddBottleCommand>();
            Register<SpendGoldCommand>();
            Register<SetFlagCommand>();
            Register<ChangeHealthCommand>();
            Register<SceneTransitionCommand>();
        }

        public static void Register<T>() where T : IGameCommand, new()
        {
            string typeName = typeof(T).Name;
            _registry[typeName] = json =>
            {
                var cmd = JsonUtility.FromJson<T>(json);
                return cmd;
            };
        }

        public static IGameCommand Deserialize(GameCommandData data)
        {
            if (_registry.TryGetValue(data.type, out var factory))
                return factory(data.json);
            Debug.LogWarning($"[SaveManager] Unknown command type: {data.type}");
            return null;
        }
    }

    // ============================================================
    // Concrete Commands
    // ============================================================

    [Serializable]
    public class AddGoldCommand : IGameCommand
    {
        public int amount;
        public AddGoldCommand() { }
        public AddGoldCommand(int a) => amount = a;
        public void Execute() { PlayerItemManager.Instance?.CmdAddGold(amount); }
        public GameCommandData Serialize() => new() { type = "AddGoldCommand", json = JsonUtility.ToJson(this), timestamp = DateTime.Now.Ticks };
    }

    [Serializable]
    public class SpendGoldCommand : IGameCommand
    {
        public int amount;
        public SpendGoldCommand() { }
        public SpendGoldCommand(int a) => amount = a;
        public void Execute() { PlayerItemManager.Instance?.CmdSpendGold(amount); }
        public GameCommandData Serialize() => new() { type = "SpendGoldCommand", json = JsonUtility.ToJson(this), timestamp = DateTime.Now.Ticks };
    }

    [Serializable]
    public class AddBottleCommand : IGameCommand
    {
        public int amount;
        public AddBottleCommand() { }
        public AddBottleCommand(int a) => amount = a;
        public void Execute() { PlayerItemManager.Instance?.CmdAddBottle(amount); }
        public GameCommandData Serialize() => new() { type = "AddBottleCommand", json = JsonUtility.ToJson(this), timestamp = DateTime.Now.Ticks };
    }

    [Serializable]
    public class SetFlagCommand : IGameCommand
    {
        public string flagId;
        public bool value;
        public SetFlagCommand() { }
        public SetFlagCommand(string id, bool v) { flagId = id; value = v; }
        public void Execute() { GameState.SetFlag(flagId, value); }
        public GameCommandData Serialize() => new() { type = "SetFlagCommand", json = JsonUtility.ToJson(this), timestamp = DateTime.Now.Ticks };
    }

    [Serializable]
    public class ChangeHealthCommand : IGameCommand
    {
        public float delta;
        public ChangeHealthCommand() { }
        public ChangeHealthCommand(float d) => delta = d;
        public void Execute()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var c = player.GetComponent<Character>();
                if (c != null) c.HealHealth(delta);
            }
        }
        public GameCommandData Serialize() => new() { type = "ChangeHealthCommand", json = JsonUtility.ToJson(this), timestamp = DateTime.Now.Ticks };
    }

    [Serializable]
    public class SceneTransitionCommand : IGameCommand
    {
        public string sceneName;
        public SceneTransitionCommand() { }
        public SceneTransitionCommand(string s) => sceneName = s;
        public void Execute() { } // 由加载流程处理，不需要在重放时切换场景
        public GameCommandData Serialize() => new() { type = "SceneTransitionCommand", json = JsonUtility.ToJson(this), timestamp = DateTime.Now.Ticks };
    }
}
