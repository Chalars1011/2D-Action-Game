namespace GameArchitecture.Narrative
{
    /// <summary>
    /// 可持久化的数据接口。
    /// 每个需要存档的系统实现这个接口。
    /// 
    /// Capture: 系统把自己的当前状态打包返回
    /// Restore: 系统从存档数据恢复自己的状态
    /// 
    /// SaveManager 在存档时遍历所有实现者调 Capture，
    /// 读档时遍历所有实现者调 Restore。
    /// 系统自己不需要知道底层存到了 PlayerPrefs 还是 JSON。
    /// </summary>
    public interface IStateTracked
    {
        string StateId { get; }
        string CaptureState();
        void RestoreState(string json);
    }
}
