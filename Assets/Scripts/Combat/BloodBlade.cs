using UnityEngine;
using GameArchitecture.Combat;

/// <summary>
/// 血刃——远程飞行投射物。
/// 
/// 伤害机制（两种配置方式，二选一）：
/// 
/// 方式A（默认，当前预制体）：
///   BloodBlade 预制体上同时挂 BloodBlade.cs + AttackBase.cs。
///   BloodBlade 管飞行和碰撞动画，AttackBase 管伤害判定。
///   BloodBlade 的 damage 字段仅作参考/调试，实际伤害由 AttackBase.atk 决定。
/// 
/// 方式B（推荐，需改预制体）：
///   移除 AttackBase 组件，设 handleDamageDirectly = true。
///   BloodBlade 自己调用 DamageCalculator 造成伤害，
///   伤害值由 damage 字段配置。
/// </summary>
public class BloodBlade : MonoBehaviour
{
    [Header("Flight")]
    public float flySpeed = 20f;
    public float lifeTime = 3f;
    public string enemyTag = "Enemy";

    [Header("Damage (used when handleDamageDirectly = true)")]
    public int damage = 25;
    public bool isHeavyHit = false;
    public DamageType damageType = DamageType.Physical;

    [Header("Mode")]
    [Tooltip("勾选后 BloodBlade 自己造成伤害（需从预制体移除 AttackBase）")]
    public bool handleDamageDirectly = false;

    private Animator _animator;
    private Rigidbody2D _rb;
    private bool _isHit;
    private int _attackId;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        // 如果预制体上有 AttackBase，同步伤害值
        if (!handleDamageDirectly)
        {
            var atk = GetComponent<AttackBase>();
            if (atk != null)
            {
                atk.atk = damage;
                atk.isHeavyHit = isHeavyHit;
            }
        }
    }

    private void OnEnable()
    {
        _isHit = false;
        _attackId = HitTracker.BeginAttack();
        _rb.velocity = Vector2.zero;
        Invoke(nameof(DestroyBlade), lifeTime);
    }

    private void OnDisable()
    {
        HitTracker.EndAttack(_attackId);
    }

    private void FixedUpdate()
    {
        if (!_isHit)
            _rb.velocity = transform.right * flySpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
        {
            // Way A: AttackBase on same prefab handles damage
            // Way B: BloodBlade handles it directly
            if (handleDamageDirectly)
            {
                if (!HitTracker.TryHit(_attackId, other.GetInstanceID()))
                    return;

                Character targetChar = other.GetComponent<Character>();
                if (targetChar != null)
                {
                    var input = new DamageInput
                    {
                        baseDamage = damage,
                        damageType = damageType,
                        attackerInstanceId = GetInstanceID(),
                        targetInstanceId = targetChar.GetInstanceID(),
                        isHeavyHit = isHeavyHit,
                        knockbackDirection = (other.transform.position - transform.position).normalized
                    };

                    var result = DamageCalculator.Calculate(input,
                        targetChar.currentHealth, targetChar.maxHealth);
                    targetChar.currentHealth -= result.finalDamage;
                }
            }

            _isHit = true;
            flySpeed = 0f;
            if (_animator != null)
                _animator.SetTrigger("Hit");
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            _isHit = true;
            flySpeed = 0f;
            if (_animator != null)
                _animator.SetTrigger("Hit");
        }
    }

    public void DestroyBlade()
    {
        Destroy(gameObject);
    }
}
