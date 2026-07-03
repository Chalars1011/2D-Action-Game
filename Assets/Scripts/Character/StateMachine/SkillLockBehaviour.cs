using UnityEngine;

/// <summary>
/// 挂在三个技能动画状态上。
/// OnStateEnter → 标记技能释放中，阻止跳跃和受伤动画打断技能。
/// OnStateExit  → 清除技能标记。
/// </summary>
public class SkillLockBehaviour : StateMachineBehaviour
{
    private PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.isSkillActive = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null) return;
        pc.isSkillActive = false;
    }
}
