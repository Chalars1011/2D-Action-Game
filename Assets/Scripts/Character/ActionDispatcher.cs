using UnityEngine;

namespace GameArchitecture.Actor
{
    /// <summary>
    /// 动作调度器——统一管理当前动作的优先级和打断规则。
    /// 
    /// 替代原来散落在 PlayerController 各处的：
    ///   if(isAttack || isSkillActive || isHurt || isDodging) return;
    /// 
    /// 改为集中管理：
    ///   if(!dispatcher.TryStartAction(ActionPriority.Jump)) return;
    /// </summary>
    public class ActionDispatcher
    {
        /// <summary>
        /// 当前正在执行的动作的优先级。
        /// Idle 表示没有任何动作在执行。
        /// </summary>
        public ActionPriority CurrentPriority { get; private set; } = ActionPriority.Idle;

        /// <summary>
        /// 当前动作已经持续了多长时间（秒）。
        /// </summary>
        public float CurrentActionTime { get; private set; }

        /// <summary>
        /// 尝试开始一个动作。
        /// 如果当前有更高优先级的动作在执行，返回 false。
        /// 如果成功，更新当前优先级并重置计时器。
        /// 
        /// 返回 true = 可以执行，false = 被更高优先级动作阻止。
        /// </summary>
        public bool TryStartAction(ActionPriority priority)
        {
            // 只有优先级更高（数值更小）的动作才能打断当前动作
            if (priority >= CurrentPriority && CurrentPriority != ActionPriority.Idle)
                return false;

            ForceStartAction(priority);
            return true;
        }

        /// <summary>
        /// 强制开始一个动作，不检查优先级。
        /// 用于必须无条件切换的场景（如动画事件结束、死亡触发等）。
        /// </summary>
        public void ForceStartAction(ActionPriority priority)
        {
            CurrentPriority = priority;
            CurrentActionTime = 0f;
        }

        /// <summary>
        /// 结束当前动作，恢复到 Idle 状态。
        /// 由动画事件或逻辑在动作完成时调用。
        /// </summary>
        public void EndCurrentAction()
        {
            CurrentPriority = ActionPriority.Idle;
            CurrentActionTime = 0f;
        }

        /// <summary>
        /// 检查当前是否有动作在执行（不是 Idle）。
        /// </summary>
        public bool IsBusy => CurrentPriority != ActionPriority.Idle;

        /// <summary>
        /// 检查是否可以执行指定优先级的动作（不实际执行）。
        /// </summary>
        public bool CanStartAction(ActionPriority priority)
        {
            if (CurrentPriority == ActionPriority.Idle) return true;
            return priority < CurrentPriority;
        }

        /// <summary>
        /// 每帧调用，更新当前动作的持续时间。
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (IsBusy)
                CurrentActionTime += deltaTime;
        }

        /// <summary>
        /// 调试用：获取当前状态描述。
        /// </summary>
        public string GetDebugInfo()
        {
            return IsBusy
                ? $"Action: {CurrentPriority} ({CurrentActionTime:F2}s)"
                : "Idle";
        }
    }
}
