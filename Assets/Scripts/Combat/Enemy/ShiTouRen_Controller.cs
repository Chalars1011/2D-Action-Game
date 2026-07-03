using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiTouRen_Controller : EnemyBase
{
    [Header("精英技能 - 黑洞蓄力")]
    public bool isElite;
    public float skillHealthThreshold = 0.6f;
    public float skillChargeTime = 3f;
    public float skillMaxDuration = 7f;
    public float skillPullForce = 4f;
    public float skillShakeIntensity = 0.15f;
    public float skillCooldown = 15f;
    public Transform pullCenter;
    public float pullDeadZone = 0.3f;

    [Header("蓄力音效")]
    public AudioClip chargeLoopSound;
    public float chargeSoundVolume = 0.5f;
    private AudioSource chargeAudioSource;

    private bool skillActive;
    private float skillCooldownTimer;
    private Character character;

    protected override void Awake()
    {
        base.Awake();
        character = GetComponent<Character>();

        if (chargeLoopSound != null)
        {
            chargeAudioSource = gameObject.AddComponent<AudioSource>();
            chargeAudioSource.spatialBlend = 1f;
            chargeAudioSource.loop = true;
            chargeAudioSource.playOnAwake = false;
            chargeAudioSource.minDistance = 2f;
            chargeAudioSource.maxDistance = 25f;
        }

        if (isElite)
            StartCoroutine(EliteSkillWatcher());
    }

    // 协程监控血量，触发技能（不用Update，避免覆盖基类）
    private IEnumerator EliteSkillWatcher()
    {
        while (true)
        {
            yield return null;

            if (skillCooldownTimer > 0)
                skillCooldownTimer -= Time.deltaTime;

            if (!skillActive && !isDead && skillCooldownTimer <= 0 && character != null)
            {
                float hpPercent = (float)character.currentHealth / character.maxHealth;
                if (hpPercent <= skillHealthThreshold && hpPercent > 0)
                {
                    StartCoroutine(EliteSkillRoutine());
                }
            }
        }
    }

    private float originalMass;
    private RigidbodyConstraints2D originalConstraints;

    private IEnumerator EliteSkillRoutine()
    {
        skillActive = true;
        isAttacking = true;
        currentSpeed = 0f;
        anim.SetTrigger("charge");
        EffectPoolManager.Instance.StartAmbientShake(skillShakeIntensity, 15f);

        // 蓄力音效
        if (chargeAudioSource != null && chargeLoopSound != null)
        {
            chargeAudioSource.clip = chargeLoopSound;
            chargeAudioSource.volume = chargeSoundVolume;
            chargeAudioSource.Play();
        }

        // 定在原地，防止被玩家推着走
        originalMass = rb.mass;
        originalConstraints = rb.constraints;
        rb.mass = 9999f;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        float timer = 0f;

        while (timer < skillMaxDuration && !isDead)
        {
            // 面朝玩家
            if (attckTarget != null)
            {
                if (attckTarget.position.x > transform.position.x)
                    transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
                else
                    transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
            }

            timer += Time.deltaTime;

            // 吸力从0逐渐升到最大，蓄力完成时达到峰值
            float pullMultiplier = Mathf.Clamp01(timer / skillChargeTime);
            float currentPull = skillPullForce * pullMultiplier;

            if (attckTarget != null)
            {
                Rigidbody2D playerRb = attckTarget.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector3 center = pullCenter != null ? pullCenter.position : transform.position;
                    Vector2 pullDir = (center - attckTarget.position).normalized;
                    float dist = Vector3.Distance(center, attckTarget.position);

                    // 到达中心点附近速度清零，防止惯性飞出去
                    if (dist <= pullDeadZone)
                    {
                        playerRb.velocity = Vector2.zero;
                    }
                    else
                    {
                        playerRb.AddForce(pullDir * currentPull, ForceMode2D.Force);
                    }
                }
            }

            // 蓄力完成后，玩家进范围就攻击
            if (timer >= skillChargeTime && attckTarget != null)
            {
                float dist = Vector3.Distance(transform.position, attckTarget.position);
                if (dist <= attackRrange)
                    break;
            }

            yield return null;
        }

        // 释放攻击，停止底震和蓄力音效，恢复物理属性
        if (chargeAudioSource != null) chargeAudioSource.Stop();
        EffectPoolManager.Instance.StopAmbientShake();
        rb.mass = originalMass;
        rb.constraints = originalConstraints;
        anim.SetTrigger("attack");
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
        skillActive = false;
        skillCooldownTimer = skillCooldown;
    }

    protected override void AttackLogic()
    {
        if (skillActive) return;
        base.AttackLogic();
    }

    public void FreezeTime()
    {
        EffectPoolManager.Instance.FreezeTime(0.1f, 0.3f);
    }

    public void ShakeScreen()
    {
        EffectPoolManager.Instance.ShakeScreen(3, 0.4f, 30);
    }
}
