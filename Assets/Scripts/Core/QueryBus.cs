using System;
using System.Collections.Generic;

namespace GameArchitecture.Core
{
    /// <summary>
    /// 查询总线。与 EventBus 的区别：
    /// - EventBus 是"通知"（无返回值，发出就完）
    /// - QueryBus 是"查询"（聚合返回值，用于计算最终结果）
    /// 
    /// 典型场景：伤害计算。
    /// 攻击方 Buff、防御方抗性、部位倍率...各自注册 Handler，
    /// DamageCalculator 通过 QueryBus 收集所有修正倍率后相乘。
    /// </summary>
    public static class QueryBus
    {
        private static readonly Dictionary<Type, List<object>> _handlers
            = new Dictionary<Type, List<object>>();

        /// <summary>
        /// 注册一个查询处理器。
        /// 通常在 Effect 的 OnApply 中注册，在 OnRemove 中取消注册。
        /// </summary>
        public static void Register<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler)
            where TQuery : struct
        {
            var key = GetKey<TQuery, TResult>();
            if (!_handlers.ContainsKey(key))
                _handlers[key] = new List<object>();
            _handlers[key].Add(handler);
        }

        /// <summary>
        /// 取消注册。必须在 Effect 被移除时调用，否则会残留幽灵 Handler。
        /// </summary>
        public static void Unregister<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler)
            where TQuery : struct
        {
            var key = GetKey<TQuery, TResult>();
            if (_handlers.ContainsKey(key))
                _handlers[key].Remove(handler);
        }

        /// <summary>
        /// 收集所有处理器的返回值。
        /// 返回一个全新的列表，调用方可以自由修改。
        /// </summary>
        public static List<TResult> Collect<TQuery, TResult>(TQuery query)
            where TQuery : struct
        {
            var results = new List<TResult>();
            var key = GetKey<TQuery, TResult>();

            if (!_handlers.ContainsKey(key)) return results;

            foreach (var obj in _handlers[key])
            {
                try
                {
                    var handler = (IQueryHandler<TQuery, TResult>)obj;
                    results.Add(handler.Handle(query));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(
                        $"[QueryBus] 查询处理器 {obj.GetType().Name} 抛出异常: {ex}");
                }
            }

            return results;
        }

        /// <summary>
        /// 乐观合并：只要有一个处理器返回了"正向"结果，立刻返回。
        /// 用于"问一圈，谁有答案就用谁的"的场景。
        /// 例：查询"是否阻止玩家输入"——只要有一个系统说"是"，结果就是 true。
        /// </summary>
        public static TResult CollectOptimistic<TQuery, TResult>(
            TQuery query,
            Func<TResult, bool> isPositive)
            where TQuery : struct
            where TResult : struct
        {
            var key = GetKey<TQuery, TResult>();
            if (!_handlers.ContainsKey(key)) return default;

            foreach (var obj in _handlers[key])
            {
                try
                {
                    var handler = (IQueryHandler<TQuery, TResult>)obj;
                    var result = handler.Handle(query);
                    if (isPositive(result)) return result;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[QueryBus] 查询处理器异常: {ex}");
                }
            }

            return default;
        }

        /// <summary>
        /// 清空所有处理器。仅在游戏完全重置时调用。
        /// </summary>
        public static void Clear()
        {
            _handlers.Clear();
        }

        private static Type GetKey<TQuery, TResult>()
        {
            return typeof(ValueTuple<TQuery, TResult>);
        }
    }

    /// <summary>
    /// 查询处理器接口。
    /// TQuery: 查询请求类型（struct，避免 GC）
    /// TResult: 返回结果类型
    /// </summary>
    public interface IQueryHandler<TQuery, TResult> where TQuery : struct
    {
        TResult Handle(TQuery query);
    }
}
