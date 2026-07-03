namespace GameArchitecture.Actor
{
    /// <summary>
    /// 玩家输入意图——不是物理按键，而是"玩家想做什么"。
    /// 
    /// 这层抽象的含义：
    /// "玩家想要做什么"和"按了什么键"是两个不同的问题。
    /// 代码使用 GameAction 而不是 KeyCode，意味着：
    /// 1. 换输入设备不需要改逻辑代码
    /// 2. 输入缓冲可以基于意图而非按键
    /// 3. 测试可以模拟任意输入序列
    /// </summary>
    public enum GameAction
    {
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        Jump,
        Attack,
        Skill1,
        Skill2,
        Skill3,
        Dodge,
        Heal,
        Interact,
        UseItem,
        Pause
    }
}
