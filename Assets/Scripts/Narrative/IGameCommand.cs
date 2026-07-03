namespace GameArchitecture.Narrative
{
    /// <summary>
    /// 游戏命令接口——所有改变持久状态的操作必须走命令模式。
    /// 
    /// 为什么不用直接修改？
    ///   直接修改 → 存档时必须知道"所有被修改的状态在哪里"
    ///   命令模式 → 存档只需保存命令序列，加载时重放
    /// 
    /// 所有改金币、改血量、开门、推进任务等操作都实现这个接口。
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>执行命令——修改游戏状态。</summary>
        void Execute();

        /// <summary>序列化为可存储的格式。</summary>
        GameCommandData Serialize();
    }

    /// <summary>
    /// 命令的序列化数据——存到 JSON 里。
    /// </summary>
    [System.Serializable]
    public struct GameCommandData
    {
        public string type;      // 命令类型，反序列化时用于查找对应的命令类
        public string json;      // 命令参数（JSON）
        public long timestamp;   // 时间戳
    }
}
