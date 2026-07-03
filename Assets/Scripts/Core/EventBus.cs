using System;
using System.Collections.Generic;

namespace GameArchitecture.Core
{
    /// <summary>
    /// 全局事件总线。所有跨模块的"通知"都通过这里传递。
    /// 
    /// 使用规则：
    /// 1. 事件类型必须是 struct（避免 GC 分配）
    /// 2. 事件类型名以 Event 结尾（约定）
    /// 3. 订阅者必须在 OnDisable/OnDestroy 中取消订阅
    /// 4. 不要在事件回调中再次 Emit 同类型事件（会被拦截并警告）
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers
            = new Dictionary<Type, List<Delegate>>();

        private static readonly HashSet<Type> _emittingTypes = new HashSet<Type>();

        public static IDisposable On<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(handler);
            return new Subscription<T>(handler, type);
        }

        public static void Off<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type)) return;
            _subscribers[type].Remove(handler);
        }

        public static void Emit<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type)) return;

            if (_emittingTypes.Contains(type))
            {
                UnityEngine.Debug.LogWarning(
                    $"[EventBus] 检测到递归发射 {type.Name}，" +
                    $"已忽略本次发射。请检查事件处理器的循环依赖。");
                return;
            }

            _emittingTypes.Add(type);

            var handlers = _subscribers[type].ToArray();

            foreach (Action<T> handler in handlers)
            {
                try
                {
                    handler(evt);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(
                        $"[EventBus] 事件 {type.Name} 的处理器抛出异常：" +
                        $"\n处理器: {handler.Method.DeclaringType}.{handler.Method.Name}" +
                        $"\n异常: {ex}");
                }
            }

            _emittingTypes.Remove(type);
        }

        public static void Clear()
        {
            _subscribers.Clear();
            _emittingTypes.Clear();
        }

        public static int SubscriberCount
        {
            get
            {
                int count = 0;
                foreach (var list in _subscribers.Values)
                    count += list.Count;
                return count;
            }
        }

        private class Subscription<T> : IDisposable where T : struct
        {
            private Action<T> _handler;
            private Type _type;

            public Subscription(Action<T> handler, Type type)
            {
                _handler = handler;
                _type = type;
            }

            public void Dispose()
            {
                if (_handler != null)
                {
                    Off<T>(_handler);
                    _handler = null;
                }
            }
        }
    }
}
