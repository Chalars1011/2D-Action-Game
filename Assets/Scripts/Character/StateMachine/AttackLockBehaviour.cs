using UnityEngine;

/// <summary>
/// 挂在攻击动画状态上。
/// OnStateEnter → 标记攻击中、禁止移动，防止玩家在攻击期间走动。
/// OnStateExit  → 恢复攻击标记和移动能力。
/// </summary>
public class AttackLockBehaviour : StateMachineBehaviour
{
    private PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.isAttack = true;
        pc.canRun = false;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null) return;
        pc.isAttack = false;
        pc.canRun = true;
    }
}
