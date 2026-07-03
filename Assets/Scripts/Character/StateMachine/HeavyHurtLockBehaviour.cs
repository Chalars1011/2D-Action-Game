using UnityEngine;

/// <summary>
/// 挂在严重受伤/击飞动画状态上。
/// OnStateEnter → 进入受击硬直期间忽略与敌人层的碰撞，防止被连续攻击。
///                  PlayerController.FixedUpdate 在 isHurt 为 true 时自动维护碰撞忽略。
/// OnStateExit  → 标记通过 HurtRecoverEnd 在爬起动画结束后统一恢复。
///                 这里不做重置，避免与爬起动画的衔接产生冲突。
/// </summary>
public class HeavyHurtLockBehaviour : StateMachineBehaviour
{
    private PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponent<PlayerController>();
        if (pc == null) return;

        // 严重受伤期间保持 isHurt 为 true，FixedUpdate 会自动忽略敌人碰撞
        pc.isHurt = true;
    }

    // OnStateExit 不做重置。
    // 严重受伤后衔接"爬起动画"，由 HurtRecoverEnd 统一清理 isHurt 和碰撞层。
}
