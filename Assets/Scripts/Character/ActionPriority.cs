namespace GameArchitecture.Actor
{
    /// <summary>
    /// 动作优先级——数值越小优先级越高。
    /// 高优先级动作可以打断低优先级动作（反之不行）。
    /// 
    /// 用法：
    ///   不再写 if(isAttack||isSkillActive||isHurt) return;
    ///   改为    if(!ActionDispatcher.CanInterrupt(ActionPriority.Jump)) return;
    /// </summary>
    public enum ActionPriority
    {
        Cutscene    = 0,    // 过场动画——最高，禁止一切操作
        Death       = 10,   // 死亡——触发死亡流程
        HitStun     = 20,   // 受击硬直——被击中时的硬直/击飞
        HeavyHurt   = 25,   // 重度受伤——击倒/击飞
        Skill       = 40,   // 技能释放——不可自行中断的高优先级动作
        Attack      = 50,   // 普通攻击
        Item        = 55,   // 使用道具（血刃/无敌/召唤）
        Dodge       = 60,   // 闪避
        Jump        = 65,   // 跳跃
        Heal        = 70,   // 回血
        Interact    = 75,   // 交互（开门/对话/拾取）
        Move        = 80,   // 移动——最低优先级，任何动作都可打断
        Idle        = 100   // 待机
    }
}
