using UnityEngine;

/// <summary>
/// 挂在玩家死亡动画状态上。
/// OnStateEnter → 标记死亡状态，禁用刚体物理模拟，关闭碰撞体。
///                 死亡后不再恢复，因此 OnStateExit 不做处理。
/// </summary>
public class DeathLockBehaviour : StateMachineBehaviour
{
    private PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.isDie = true;

        if (pc.rb != null)
            pc.rb.simulated = false;

        // 关掉所有碰撞体，死亡后不再受碰撞影响
        Collider2D[] cols = pc.GetComponents<Collider2D>();
        foreach (var col in cols)
            col.enabled = false;
    }
}
