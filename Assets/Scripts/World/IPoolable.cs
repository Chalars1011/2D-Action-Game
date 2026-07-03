namespace GameArchitecture.World
{
    /// <summary>
    /// 可池化对象接口。
    /// 实现此接口的对象在从对象池取出/归还时会收到通知。
    /// 
    /// 为什么需要？
    /// GameObject.SetActive(false) 只停用渲染和物理，不重置状态。
    /// 比如：子弹的速度方向还在、粒子在播放、碰撞标记在。
    /// 不重置的话，下次取出带着旧状态 → bug。
    /// </summary>
    public interface IPoolable
    {
        /// <summary>从池中取出时调用。在这里初始化/激活。</summary>
        void OnTakenFromPool();

        /// <summary>归还到池中时调用。在这里清理/停用。</summary>
        void OnReturnedToPool();
    }
}
