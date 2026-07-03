using UnityEngine;

/// <summary>
/// 挂在普���受伤动画状态上。
/// OnStateEnter → 标记受伤中，禁止攻击和闪避。
/// OnStateExit  → 清除受伤标记，恢复操作。
/// </summary>
public class HurtLockBehaviour : StateMachineBehaviour
{
    private PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.isHurt = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null) return;
        pc.isHurt = false;
    }
}
