using UnityEngine;

public class ClimbSpeedController : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float climbSpeed = animator.GetFloat("ClimbSpeed");
        if (climbSpeed < -0.1f)
            animator.Play(stateInfo.fullPathHash, layerIndex, 1f);
    }
}
