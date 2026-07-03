using UnityEngine;
using GameArchitecture.Combat;

/// <summary>
/// 【旧架构】攻击判定 + 表现（震屏/音效/血特效 全部耦合）。
///
/// ⚠️ 此组件正在逐步废弃，新代码请使用：
///   - HitboxComponent  → 纯碰撞检测 + 伤害（万能）
///   - HitPresentation  → 震屏/音效/血特效（仅玩家攻击）
///
/// 分离后敌人攻击不需要震屏参数，加新特效不需要改判定逻辑。
/// 当前此组件仍保留以确保旧攻击特效预制体兼容。
/// </summary>
public class AttackBase : MonoBehaviour
{
    public int atk;
    private int directionValue;
    public string targetTag;
    public EffectType effectType;
    protected bool canDamage = false;
    public bool isHeavyHit;
    public enum EffectType { Effect1, Effect2, Effect3 }

    [Header("命中音效")]
    public AudioClip hitSound;
    public float hitSoundVolume = 0.8f;

    [Header("屏幕震动")]
    [Tooltip("优先使用 ShakeProfile 资产")]
    public ShakeProfile_SO shakeProfile;
    public float ShakeScreenIntensity;
    public float ShakeScreenDuration;
    public float ShakeScreenFrequency;

    [Header("时间冻结")]
    public bool CanFreezeTime;
    public float FreezeTimeIntensity;
    public float FreezeTimeDuration;

    // === 架构 v2：命中去重 ===
    private int _attackId;

    private void OnEnable()
    {
        canDamage = true;
        _attackId = HitTracker.BeginAttack();
    }

    private void OnDisable()
    {
        canDamage = false;
        HitTracker.EndAttack(_attackId);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDamage) return;
        if (!IsValidTarget(collision)) return;

        // === 命中去重：同一次攻击不伤害同一目标两次 ===
        int targetId = collision.GetInstanceID();
        if (!HitTracker.TryHit(_attackId, targetId))
            return;

        AttackTotalEffect(collision);
        canDamage = false; // 保持原有"一次攻击命中后失活"行为
    }

    protected void AttackTotalEffect(Collider2D collision)
    {
        // 先扣血再震屏冻结，不依赖BloodTransform
        collision.GetComponent<Character>()?.TakeDmage(this);
        if (hitSound != null)
            AudioManager.Instance.PlayAtPoint(hitSound, collision.transform.position, hitSoundVolume, AudioCategory.Hit);
        if (shakeProfile != null && CameraShaker.Instance != null)
            CameraShaker.Instance.Play(shakeProfile);
        else
            EffectPoolManager.Instance.ShakeScreen(ShakeScreenIntensity, ShakeScreenDuration, ShakeScreenFrequency);
        if (CanFreezeTime)
            EffectPoolManager.Instance.FreezeTime(FreezeTimeDuration, FreezeTimeIntensity);

        // 血特效独立处理，没有BloodTransform就跳过
        Transform bloodTransform = collision.GetComponent<Character>()?.BloodTransform;
        if (bloodTransform == null) return;

        Vector2 directionVector = (collision.transform.position - transform.position).normalized;
        directionValue = directionVector.x < -0.1f ? -1 : (directionVector.x > 0.1f ? 1 : 0);

        switch (effectType)
        {
            case EffectType.Effect1:
                EffectPoolManager.Instance.BloodEffect_1(bloodTransform, directionValue);
                break;
            case EffectType.Effect2:
                EffectPoolManager.Instance.BloodEffect_2(bloodTransform, directionValue);
                break;
            case EffectType.Effect3:
                EffectPoolManager.Instance.BloodEffect_3(bloodTransform, directionValue);
                break;
        }
    }

    protected virtual bool IsValidTarget(Collider2D collision)
    {
        if (string.IsNullOrEmpty(targetTag)) return false;
        if (string.IsNullOrEmpty(collision.tag)) return false;

        return collision.CompareTag(targetTag) &&
               collision.GetComponent<Character>() != null;
    }
}
