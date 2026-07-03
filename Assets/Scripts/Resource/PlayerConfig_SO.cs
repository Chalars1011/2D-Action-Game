using UnityEngine;

/// <summary>
/// 玩家全部可调参数——集中在一个 ScriptableObject 中。
/// 修改手感不需要打开 PlayerController.cs，直接在 Inspector 里调。
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Player Config")]
public class PlayerConfig_SO : ScriptableObject
{
    [Header("移动")]
    public float maxSpeed = 5f;
    public float acceleration = 1f;     // 预留：后续从硬编码改为曲线

    [Header("跳跃")]
    public float jumpForce = 10f;
    public int maxJumpCount = 2;
    public float coyoteTime = 0.15f;       // 土狼时间：离开边缘后仍可跳跃的宽限期
    public float jumpBufferTime = 0.15f;   // 输入缓冲：提前按跳的有效时间

    [Header("下蹲")]
    public float crouchColliderOffsetY = 0.5f;
    public float crouchColliderHeight = 1.0f;

    [Header("闪避")]
    public float dodgeShortDistance = 3f;
    public float dodgeLongDistance = 6f;
    public float dodgeChargeThreshold = 0.08f; // 长按判定秒数
    public float dodgeDashDuration = 0.1f;     // 冲刺耗时
    public float dodgeCooldown = 1.5f;

    [Header("技能蓝量消耗")]
    public float skill1ManaCost = 20f;
    public float skill2ManaCost = 30f;
    public float skill3ManaCost = 50f;

    [Header("技能2冲刺")]
    public float skill2DashDistance = 5f;
    public float skill2DashDuration = 0.12f;

    [Header("受伤/击退")]
    public float hurtForce = 8f;
    public float hurtDuration = 0.3f;
    public float heavyHurtForceMultiplier = 1.5f;
    public float heavyHurtUpwardMultiplier = 1.2f;

    [Header("无敌")]
    public float invincibleDuration = 5f;
    public Color invincibleColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Header("钉墙")]
    public float nailSlideSpeed = 1f;
    public float nailJumpForce = 8f;

    [Header("攀爬")]
    public float climbSpeed = 3f;
    public float edgeClimbSpeed = 3f;

    [Header("地面检测")]
    public float groundCheckRadius = 0.2f;

    // ============================================================
    // 验证：在编辑器中修改参数时自动检查合法性
    // ============================================================
    private void OnValidate()
    {
        maxSpeed = Mathf.Max(0.1f, maxSpeed);
        jumpForce = Mathf.Max(0.1f, jumpForce);
        maxJumpCount = Mathf.Max(1, maxJumpCount);
        dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
        dodgeDashDuration = Mathf.Max(0.01f, dodgeDashDuration);
        skill1ManaCost = Mathf.Max(0, skill1ManaCost);
        skill2ManaCost = Mathf.Max(0, skill2ManaCost);
        skill3ManaCost = Mathf.Max(0, skill3ManaCost);
        hurtForce = Mathf.Max(0, hurtForce);
        groundCheckRadius = Mathf.Max(0.01f, groundCheckRadius);
    }
}
