using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStates
{
    XunLuo,
    ZhuiJi,
    Death,
    Attack
}

public class EnemyBase : MonoBehaviour
{
    protected Rigidbody2D rb;
    public Animator anim;
    [HideInInspector]
    public PhysicsCheck physicsCheck;
    private EnemyStates enemyStates;
    protected Transform attckTarget;
    private Character enemyCharacter;
    private Collider2D[] colliders;

    [Header("=== 架构 v2：ScriptableObject 配置（拖入后覆盖 Inspector 值）===")]
    [SerializeField] private EnemyConfig_SO config;

    [Header("掉落物品")]
    public int coinDropAmount = 3;
    public float coinSpawnRadius = 1.5f;
    public float coinSpawnRandomness = 0.3f;
    public Transform creatPosition;

    [Header("移动参数")]
    public float normalSpeed;
    public float chaseSpeed;
    public float chaseSpeedMultiplier = 1.5f;
    public float currentSpeed;
    public Vector2 facedir;

    [Header("朝向")]
    public Vector3 faceDir;

    [Header("等待计时")]
    public float waitTime;
    public float waitTimeCounter;
    public bool Wait;

    [Header("攻击")]
    Transform attacker;
    public bool isDead;

    [Header("攻击状态")]
    public float attackRrange;
    public float attackCooldown = 1.5f;
    private float attackCooldownTimer = 0f;
    private bool isInAttackRange = false;
    public bool isAttacking = false;

    [Header("检测器")]
    public Vector2 centerOfset;
    public Vector2 checkSize;
    public float checkDistance;
    public LayerMask playerLayer;
    public float lostTime;
    public float lostTimer;

    private void ApplyConfig()
    {
        if (config == null) return;

        normalSpeed = config.normalSpeed;
        chaseSpeedMultiplier = config.chaseSpeedMultiplier;
        waitTime = config.waitTime;
        lostTime = config.lostTargetTime;
        attackRrange = config.attackRange;
        attackCooldown = config.attackCooldown;
        centerOfset = config.detectionCenterOffset;
        checkSize = config.detectionBoxSize;
        checkDistance = config.detectionDistance;
        coinDropAmount = config.coinDropAmount;
        coinSpawnRadius = config.coinSpawnRadius;
        coinSpawnRandomness = config.coinSpawnRandomness;
    }

    protected Vector3 baseScale; // 保存原始缩放，精英怪放大后不回弹

    protected virtual void Awake()
    {
        // === 架构 v2：从 SO 加载配置 ===
        ApplyConfig();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        physicsCheck = GetComponent<PhysicsCheck>();
        currentSpeed = normalSpeed;
        enemyCharacter = GetComponent<Character>();
        baseScale = transform.localScale;
    }

    void Start()
    {
        waitTimeCounter = waitTime;
        lostTimer = lostTime;
        attackCooldownTimer = 0f;
        colliders = GetComponents<Collider2D>();
    }

    private void OnEnable()
    {
        isDead = false;
        enemyStates = EnemyStates.XunLuo;
    }

    void Update()
    {
        faceDir = new Vector3(transform.localScale.x, 0, 0);

        // 更新攻击冷却计时器
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        isInAttackRange = InAttackRange();
        SwitchState();
        TimeCounter();
    }

    private void FixedUpdate()
    {
        if (!isDead && !Wait && !isAttacking)
        {
            Move();
        }
    }

    private void OnDisable()
    {
        attackCooldownTimer = 0f;
    }

    public void SwitchState()
    {
        // 死亡优先级最高
        if (enemyCharacter.currentHealth <= 0)
        {
            enemyStates = EnemyStates.Death;
        }
        else if (FoundPlayer())
        {
            if (isInAttackRange)
                enemyStates = EnemyStates.Attack;
            else
                enemyStates = EnemyStates.ZhuiJi;
        }
        // 目标丢失立刻回巡逻，不等lostTimer
        else if (attckTarget == null && (enemyStates == EnemyStates.ZhuiJi || enemyStates == EnemyStates.Attack))
        {
            enemyStates = EnemyStates.XunLuo;
        }
        // 丢失玩家超时回到巡逻
        else if (lostTimer <= 0 && enemyStates == EnemyStates.ZhuiJi)
        {
            enemyStates = EnemyStates.XunLuo;
        }

        switch (enemyStates)
        {
            case EnemyStates.XunLuo:
                PatrolLogic();
                break;
            case EnemyStates.ZhuiJi:
                ChaseLogic();
                break;
            case EnemyStates.Attack:
                AttackLogic();
                break;
            case EnemyStates.Death:
                DeathLogic();
                break;
        }
    }

    // 巡逻状态逻辑
    private void PatrolLogic()
    {
        if ((!physicsCheck.isGround || physicsCheck.touchLeftWall && faceDir.x < 0) ||
            (physicsCheck.touchRightWall && faceDir.x > 0))
        {
            Wait = true;
            anim.SetBool("walk", false);
        }
        else
        {
            anim.SetBool("walk", true);
        }
        currentSpeed = normalSpeed;
    }

    // 追击状态逻辑
    protected virtual void ChaseLogic()
    {
        currentSpeed = normalSpeed * chaseSpeedMultiplier;

        if (attckTarget != null)
        {
            if (attckTarget.position.x > transform.position.x && faceDir.x < 0)
            {
                Flip();
            }
            else if (attckTarget.position.x < transform.position.x && faceDir.x > 0)
            {
                Flip();
            }
        }

        anim.SetBool("walk", true);
        anim.SetBool("attack", false);
    }

    // 攻击状态逻辑
    protected virtual void AttackLogic()
    {
        if (!isInAttackRange)
        {
            enemyStates = EnemyStates.ZhuiJi;
            return;
        }

        if (attckTarget != null)
        {
            if (attckTarget.position.x > transform.position.x && faceDir.x < 0)
            {
                Flip();
            }
            else if (attckTarget.position.x < transform.position.x && faceDir.x > 0)
            {
                Flip();
            }
        }

        if (isAttacking)
        {
            currentSpeed = 0;
            anim.SetBool("walk", false);
            return;
        }

        if (attackCooldownTimer <= 0)
        {
            // 玩家倒地时不攻击
            if (attckTarget != null)
            {
                PlayerController pc = attckTarget.GetComponent<PlayerController>();
                if (pc != null && pc.isHurt) return;
            }

            currentSpeed = 0;
            anim.SetTrigger("attack");
            attackCooldownTimer = attackCooldown;
        }
        else
        {
            currentSpeed = 0;
            anim.SetBool("walk", false);
        }
    }

    // 死亡状态逻辑
    private void DeathLogic()
    {
        currentSpeed = 0;
        if (!isDead)
        {
            OnDie();
        }
    }

    public virtual void Move()
    {
        rb.velocity = new Vector2(currentSpeed * faceDir.x, rb.velocity.y);
    }

    // 翻转
    public void Flip()
    {
        if (!isAttacking)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    // 受击回调
    public virtual void OnTakeDamage(Transform attackTrans)
    {
        attacker = attackTrans;

        // 转向攻击者，保持原始缩放
        if (attackTrans.position.x - transform.position.x > 0 && !isAttacking)
        {
            transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
        }
        if (attackTrans.position.x - transform.position.x < 0 && !isAttacking)
        {
            transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
        }
    }

    public void TimeCounter()
    {
        if (Wait)
        {
            waitTimeCounter -= Time.deltaTime;
            anim.SetBool("walk", false);
            if (waitTimeCounter <= 0)
            {
                Wait = false;
                waitTimeCounter = waitTime;
                Flip();
            }
        }

        if (!FoundPlayer() && lostTimer > 0)
        {
            lostTimer -= Time.deltaTime;
        }
        else if (FoundPlayer())
        {
            lostTimer = lostTime;
        }
    }

    protected virtual void OnDie()
    {
        isDead = true;
        anim.SetBool("dead", true);
        gameObject.layer = 2;
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }
        SpawnCoins();
    }

    protected virtual void SpawnCoins()
    {
        float angleStep = 360f / coinDropAmount;

        for (int i = 0; i < coinDropAmount; i++)
        {
            float baseAngle = i * angleStep;
            float randomAngleOffset = Random.Range(-coinSpawnRandomness * angleStep / 2,
                                                 coinSpawnRandomness * angleStep / 2);
            float finalAngle = baseAngle + randomAngleOffset;

            float angleRad = finalAngle * Mathf.Deg2Rad;
            float randomRadius = coinSpawnRadius * Random.Range(0.7f, 1.0f);

            Vector2 spawnPosition = new Vector2(
                creatPosition.position.x + Mathf.Cos(angleRad) * randomRadius,
                creatPosition.position.y + Mathf.Sin(angleRad) * randomRadius
            );

            GameObject coin = PoolManager.Instance.GetObj("LingHun");
            if (coin != null)
            {
                coin.transform.position = spawnPosition;
            }
        }
    }

    public void DestroyAfterAnimation()
    {
        Destroy(this.gameObject);
    }

    public bool FoundPlayer()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            (Vector2)transform.position + centerOfset,
            checkSize,
            0,
            faceDir,
            checkDistance,
            playerLayer);

        if (hit.collider != null)
        {
            attckTarget = hit.transform;
            return true;
        }

        attckTarget = null;
        return false;
    }

    bool InAttackRange()
    {
        if (attckTarget != null)
        {
            return Vector3.Distance(attckTarget.transform.position, transform.position) <= attackRrange;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube((Vector2)transform.position + centerOfset +
        new Vector2(checkDistance * -transform.localScale.x, 0), checkSize);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRrange);
    }
}
