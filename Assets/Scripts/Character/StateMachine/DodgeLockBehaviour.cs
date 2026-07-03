using UnityEngine;

/// <summary>
/// 挂在闪避动画状态上。
/// OnStateEnter → 标记闪避中，激活闪避无敌帧（碰撞层切换由 FixedUpdate 检查 isDodging 自动完成）。
/// OnStateExit  → 清除闪避标记，恢复碰撞层。
/// </summary>
public class DodgeLockBehaviour : StateMachineBehaviour
{
    private PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.isDodging = true;
        pc.canRun = false;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null) return;
        pc.isDodging = false;
        pc.canRun = true;
    }
}
