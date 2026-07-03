using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameArchitecture.Actor
{
    /// <summary>
    /// 输入缓冲器——解决"我按了但没反应"的问题。
    /// 
    /// 工作原理：
    /// 当玩家按下跳跃键时，记录一个时间戳。
    /// 在接下来的 bufferWindow 秒内，如果跳跃条件满足（落地了），
    /// 自动触发跳跃。
    /// 
    /// 这是现代动作游戏的标配设计。《蔚蓝》(Celeste) 如果没有输入缓冲
    /// 根本无法实现其高速精准的平台跳跃。
    /// 
    /// 用法：
    ///   buffer.Update(Time.deltaTime);      // 每帧更新
    ///   buffer.RecordAction(GameAction action);  // 记录按键
    ///   if (buffer.ConsumeAction(GameAction action)) { /* 执行 */ }
    /// </summary>
    public class InputBuffer
    {
        private readonly Dictionary<GameAction, float> _buffer = new();
        private readonly float _bufferWindow; // 缓冲窗口，典型值 0.1-0.2 秒

        /// <summary>
        /// 创建一个输入缓冲器。
        /// </summary>
        /// <param name="bufferWindow">缓冲窗口（秒）。推荐：跳跃 0.15s，攻击 0.1s</param>
        public InputBuffer(float bufferWindow = 0.15f)
        {
            _bufferWindow = bufferWindow;
        }

        /// <summary>
        /// 每帧调用——减少所有已记录缓冲的剩余时间。
        /// </summary>
        public void Tick(float deltaTime)
        {
            var expiredActions = new List<GameAction>();
            foreach (var kvp in _buffer)
            {
                _buffer[kvp.Key] -= deltaTime;
                if (_buffer[kvp.Key] <= 0)
                    expiredActions.Add(kvp.Key);
            }
            foreach (var action in expiredActions)
                _buffer.Remove(action);
        }

        /// <summary>
        /// 记录一个动作到缓冲中。
        /// 在按键按下时调用。
        /// </summary>
        public void RecordAction(GameAction action)
        {
            _buffer[action] = _bufferWindow;
        }

        /// <summary>
        /// 检查一个动作是否"有效"——在缓冲窗口内。
        /// 如果有效，消费掉缓冲（防止重复触发）。
        /// </summary>
        /// <returns>true = 动作在缓冲窗口内，应该执行</returns>
        public bool ConsumeAction(GameAction action)
        {
            if (_buffer.ContainsKey(action) && _buffer[action] > 0)
            {
                _buffer.Remove(action);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查动作是否在缓冲中（不消费）。
        /// </summary>
        public bool HasAction(GameAction action)
        {
            return _buffer.ContainsKey(action) && _buffer[action] > 0;
        }

        /// <summary>
        /// 清空所有缓冲（场景切换、死亡时调用）。
        /// </summary>
        public void Clear()
        {
            _buffer.Clear();
        }

        /// <summary>
        /// 获取缓冲中剩余时间（调试用）。
        /// </summary>
        public float GetRemainingTime(GameAction action)
        {
            return _buffer.TryGetValue(action, out float t) ? t : 0f;
        }

        public override string ToString()
        {
            var parts = new List<string>();
            foreach (var kvp in _buffer)
                if (kvp.Value > 0)
                    parts.Add($"{kvp.Key}:{kvp.Value:F3}");
            return parts.Count > 0 ? string.Join(", ", parts) : "empty";
        }
    }
}
