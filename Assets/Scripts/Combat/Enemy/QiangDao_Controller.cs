using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QiangDao_Controller : EnemyBase
{
    [Header("精英怪设置")]
    public bool isElite;
    public float reviveHealthMultiplier = 2f;
    public float reviveSpeedMultiplier = 1.3f;
    public float reviveAttackSpeedMultiplier = 1.5f;
    public Color reviveColor = Color.red;
    public int hurtResistCount = 3;

    [Header("精英技能 - 鬼步闪击")]
    public float teleportSkillCooldown = 5f;
    public float teleportDelay = 0.3f;
    public float teleportBehindDistance = 1.5f;
    public float teleportFadeTime = 0.15f;    // 淡化耗时

    private bool hasRevived;
    private int currentHurtCount;
    private float originalAttackCooldown;
    private float originalNormalSpeed;
    private float teleportSkillTimer;
    private bool skillQueued;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalAttackCooldown = attackCooldown;
        originalNormalSpeed = normalSpeed;
        StartCoroutine(EliteSkillRoutine());
    }


    // 精英技能冷却协程
    private IEnumerator EliteSkillRoutine()
    {
        while (true)
        {
            yield return null;
            if (!hasRevived) continue;
            if (teleportSkillTimer > 0)
                teleportSkillTimer -= Time.deltaTime;
            if (teleportSkillTimer <= 0 && attckTarget != null && !isDead && !skillQueued)
            {
                float dist = Vector3.Distance(transform.position, attckTarget.position);
                if (dist <= attackRrange * 2f)
                    StartTeleportSkill();
            }
        }
    }

    protected override void ChaseLogic()
    {
        currentSpeed = normalSpeed * chaseSpeedMultiplier;

        if (attckTarget != null)
        {
            if (attckTarget.position.x > transform.position.x && faceDir.x < 0)
                Flip();
            else if (attckTarget.position.x < transform.position.x && faceDir.x > 0)
                Flip();
        }

        anim.SetBool("walk", false);
        anim.SetBool("run", true);
        anim.SetBool("attack", false);
    }

    protected override void AttackLogic()
    {
        base.AttackLogic();
        anim.SetBool("run", false);

        // 精英：攻击结束后检查是否有排队技能
        if (hasRevived && skillQueued && !isAttacking && teleportSkillTimer <= 0)
        {
            skillQueued = false;
            StartCoroutine(TeleportSkill());
        }
    }

    public override void OnTakeDamage(Transform attackTrans)
    {
        base.OnTakeDamage(attackTrans);

        if (hasRevived)
        {
            currentHurtCount++;
            // 复活后受击三次才出硬直，或者重攻击直接出
            AttackBase atkBase = attackTrans.GetComponent<AttackBase>();
            bool isHeavyAttack = atkBase != null && atkBase.isHeavyHit;

            if (currentHurtCount >= hurtResistCount || isHeavyAttack)
            {
                currentHurtCount = 0;
                anim.SetTrigger("hurt");
            }
        }
        else
        {
            anim.SetTrigger("hurt");
        }

        currentSpeed = 0;
    }

    // 死亡逻辑：精英首次死亡触发复活
    protected override void OnDie()
    {
        if (isElite && !hasRevived)
        {
            // 首次死亡，不销毁，等复活动画事件调用Revive
            isDead = true;
            anim.SetBool("dead", true);
            gameObject.layer = 2;
            DisableColliders();
            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
            }
            SpawnCoins();
        }
        else
        {
            // 第二次死亡或非精英，正常销毁
            base.OnDie();
        }
    }

    // 死亡动画结束回调（动画事件调用，代替原来的DestroyAfterAnimation）
    public void OnDeathAnimEnd()
    {
        if (isElite && !hasRevived)
            Revive();
        else
            DestroyAfterAnimation();
    }

    // 复活
    private void Revive()
    {
        if (hasRevived) return;
        hasRevived = true;
        isDead = false;

        // 恢复碰撞体
        EnableColliders();
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        // 数值提升
        Character ch = GetComponent<Character>();
        if (ch != null)
        {
            ch.maxHealth *= reviveHealthMultiplier;
            ch.currentHealth = ch.maxHealth;
        }

        normalSpeed = originalNormalSpeed * reviveSpeedMultiplier;
        attackCooldown = originalAttackCooldown / reviveAttackSpeedMultiplier;
        currentHurtCount = 0;
        teleportSkillTimer = teleportSkillCooldown;

        // 颜色变化
        if (spriteRenderer != null)
            spriteRenderer.color = reviveColor;

        // 播放复活动画
        anim.SetBool("dead", false);
        anim.SetTrigger("revive");

        rb.gravityScale = 1f;
    }

    // 鬼步闪击：瞬移到玩家身后攻击
    public void StartTeleportSkill()
    {
        if (teleportSkillTimer > 0) return;

        // 如果正在攻击中，加入队列
        if (isAttacking)
        {
            skillQueued = true;
            return;
        }

        StartCoroutine(TeleportSkill());
    }

    private IEnumerator TeleportSkill()
    {
        teleportSkillTimer = teleportSkillCooldown;

        // 渐隐
        if (spriteRenderer != null)
            yield return StartCoroutine(FadeAlpha(1f, 0f, teleportFadeTime));
        DisableColliders();

        yield return new WaitForSeconds(teleportDelay);

        // 瞬移到玩家身后
        if (attckTarget != null)
        {
            Vector3 behindPos = attckTarget.position + new Vector3(
                attckTarget.localScale.x > 0 ? -teleportBehindDistance : teleportBehindDistance, 0, 0);
            transform.position = behindPos;

            if (attckTarget.position.x > transform.position.x)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // 渐显
        EnableColliders();
        if (spriteRenderer != null)
            yield return StartCoroutine(FadeAlpha(0f, 1f, teleportFadeTime));

        isAttacking = true;
        anim.SetTrigger("attack");
        attackCooldown = originalAttackCooldown / reviveAttackSpeedMultiplier;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = spriteRenderer.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = to;
        spriteRenderer.color = c;
    }

    private void DisableColliders()
    {
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (Collider2D c in cols) c.enabled = false;
    }

    private void EnableColliders()
    {
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (Collider2D c in cols) c.enabled = true;
    }

    // 提供给动画事件：攻击结束回调
    public void OnAttackEnd()
    {
        isAttacking = false;
        // 检查是否有排队技能
        if (skillQueued && teleportSkillTimer <= 0 && hasRevived)
        {
            skillQueued = false;
            StartCoroutine(TeleportSkill());
        }
    }
}
