using UnityEngine;
using System.Collections.Generic;

namespace GameArchitecture.Narrative
{
    /// <summary>GameState 的 MonoBehaviour 壳，用于向 SaveManager 注册。</summary>
    public class GameStateTracker : MonoBehaviour, IStateTracked
    {
        string IStateTracked.StateId => "GameState";

        [System.Serializable] private class FlagSnap { public List<string> flags = new(); }

        string IStateTracked.CaptureState()
        {
            var snap = new FlagSnap();
            foreach (var kv in GameState.ExportFlags())
                if (kv.Value) snap.flags.Add(kv.Key);
            return JsonUtility.ToJson(snap);
        }

        void IStateTracked.RestoreState(string json)
        {
            GameState.ResetToInitial();
            var snap = JsonUtility.FromJson<FlagSnap>(json);
            if (snap?.flags != null)
                foreach (var f in snap.flags) GameState.SetFlag(f, true);
        }

        private void Awake()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.RegisterTracker(this);
        }
    }
}
