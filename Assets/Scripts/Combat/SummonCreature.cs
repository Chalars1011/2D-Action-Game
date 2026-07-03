using GameArchitecture.Core;
using UnityEngine;

public class SummonCreature : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform followTarget;
    public float flyHeight = 3f;
    public float flyRadius = 2f;

    [Header("移动参数")]
    public float flySpeed = 1f;
    public float chaseSpeed = 3f;
    public float changeDirectionInterval = 2f;
    public float returnSpeed = 3f;

    [Header("检测范围")]
    public float detectRange = 8f;
    public float chaseRange = 5f;
    public float attackRange = 2f;

    [Header("攻击参数")]
    public float bulletSpeed = 10f;
    public int maxBulletCount = 3;
    public float bulletCooldown = 1f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public string enemyTag = "Enemy";

    [Header("死亡参数")]
    public float deathMoveSpeed = 8f;
    public float deathDelay = 3f;

    [Header("飞行音效（循环）")]
    public AudioClip flyLoopSound;
    public float flySoundVolume = 0.3f;

    private bool hasHitEnemy = false;
    private Vector3 targetPosition;
    private float timer;
    private float bulletTimer;
    private float deathDelayTimer;
    private int currentBulletCount;
    private Transform targetEnemy;
    private Animator animator;
    private AudioSource audioSource;

    private enum State { Flying, Chasing, Attacking, WaitingDeath, Dying }
    private State currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentState = State.Flying;
        currentBulletCount = 0;
        bulletTimer = 0f;
        deathDelayTimer = 0f;
        hasHitEnemy = false;

        if (flyLoopSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 15f;
        }

        SetRandomTargetPosition();
    }

    private void Update()
    {
        // 找不到玩家就持续找
        if (followTarget == null)
        {
            GameObject player = Blackboard.PlayerTransform?.gameObject;
            if (player != null) followTarget = player.transform;
        }

        if (bulletTimer > 0)
            bulletTimer -= Time.deltaTime;

        if (deathDelayTimer > 0)
            deathDelayTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Flying: HandleFlyingState(); CheckForEnemies(); break;
            case State.Chasing: HandleChasingState(); break;
            case State.Attacking: HandleAttackingState(); break;
            case State.WaitingDeath: HandleWaitingDeathState(); break;
            case State.Dying: HandleDyingState(); break;
        }

        // 飞行音效
        if (audioSource != null && flyLoopSound != null)
        {
            bool shouldPlay = currentState == State.Flying || currentState == State.Chasing || currentState == State.Attacking;
            if (shouldPlay && !audioSource.isPlaying)
            {
                audioSource.clip = flyLoopSound;
                audioSource.volume = flySoundVolume;
                audioSource.Play();
            }
            else if (!shouldPlay && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void HandleFlyingState()
    {
        timer += Time.deltaTime;
        Vector3 centerPosition = GetCenterPosition();
        float distanceFromCenter = Vector3.Distance(new Vector3(transform.position.x, transform.position.y, 0), centerPosition);
        bool isOutOfRange = distanceFromCenter > flyRadius;

        if (isOutOfRange)
        {
            Vector3 directionToCenter = (centerPosition - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, returnSpeed * Time.deltaTime);
            if (directionToCenter.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(directionToCenter.x), transform.localScale.y, transform.localScale.z);
            if (distanceFromCenter < 0.5f) { SetRandomTargetPosition(); timer = 0f; }
        }
        else
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            if (distanceToTarget < 0.3f || timer >= changeDirectionInterval)
            { SetRandomTargetPosition(); timer = 0f; }
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(direction.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private Vector3 GetCenterPosition()
    {
        if (followTarget != null)
            return new Vector3(followTarget.position.x, followTarget.position.y + flyHeight, followTarget.position.z);
        return transform.position;
    }

    private void SetRandomTargetPosition()
    {
        Vector3 centerPosition = GetCenterPosition();
        float randomX = Random.Range(-flyRadius * 0.8f, flyRadius * 0.8f);
        float randomY = Random.Range(-flyRadius * 0.5f, flyRadius * 0.5f);
        targetPosition = centerPosition + new Vector3(randomX, randomY, 0);
    }

    private void CheckForEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(enemyTag))
            {
                targetEnemy = collider.transform;
                currentState = State.Chasing;
                return;
            }
        }
    }

    private void HandleChasingState()
    {
        if (targetEnemy == null || !IsEnemyAlive(targetEnemy))
        {
            FindNewEnemy();
            if (targetEnemy == null) { currentState = State.Flying; return; }
        }

        float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
        if (distanceToEnemy <= attackRange) { currentState = State.Attacking; return; }
        if (distanceToEnemy > chaseRange) { currentState = State.Flying; return; }

        Vector3 directionToEnemy = (targetEnemy.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position, chaseSpeed * Time.deltaTime);
        if (directionToEnemy.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(directionToEnemy.x), transform.localScale.y, transform.localScale.z);
    }

    private void HandleAttackingState()
    {
        if (targetEnemy == null || !IsEnemyAlive(targetEnemy))
        {
            FindNewEnemy();
            if (targetEnemy == null) { currentState = State.Flying; return; }
        }

        float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
        if (distanceToEnemy > attackRange) { currentState = State.Chasing; return; }

        float direction = targetEnemy.position.x - transform.position.x;
        transform.localScale = new Vector3(Mathf.Sign(direction), transform.localScale.y, transform.localScale.z);

        if (bulletTimer <= 0) { FireBullet(); bulletTimer = bulletCooldown; }
    }

    private bool IsEnemyAlive(Transform enemy)
    {
        return enemy != null && enemy.gameObject.activeInHierarchy;
    }

    private void FindNewEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectRange);
        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(enemyTag))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance) { closestDistance = distance; closestEnemy = collider.transform; }
            }
        }
        targetEnemy = closestEnemy;
    }

    private void FireBullet()
    {
        if (currentBulletCount >= maxBulletCount) return;
        if (targetEnemy == null || !IsEnemyAlive(targetEnemy)) return;
        if (bulletPrefab == null || bulletSpawnPoint == null) return;

        Vector3 direction = (targetEnemy.position - bulletSpawnPoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.transform.right = direction;
        HomingBullet homingBullet = bullet.GetComponent<HomingBullet>();
        if (homingBullet != null) { homingBullet.SetTarget(targetEnemy); homingBullet.flySpeed = bulletSpeed; }
        else
        {
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = direction * bulletSpeed;
        }
        currentBulletCount++;
        if (currentBulletCount >= maxBulletCount) { deathDelayTimer = deathDelay; currentState = State.WaitingDeath; }
    }

    private void HandleWaitingDeathState()
    {
        if (targetEnemy == null || !IsEnemyAlive(targetEnemy)) FindNewEnemy();
        if (targetEnemy == null) { currentState = State.Dying; hasHitEnemy = false; return; }
        float direction = targetEnemy.position.x - transform.position.x;
        transform.localScale = new Vector3(Mathf.Sign(direction), transform.localScale.y, transform.localScale.z);
        if (deathDelayTimer <= 0) { currentState = State.Dying; hasHitEnemy = false; }
    }

    private void HandleDyingState()
    {
        if (targetEnemy == null || !IsEnemyAlive(targetEnemy)) FindNewEnemy();
        if (targetEnemy == null) return;
        if (!hasHitEnemy)
        {
            Vector3 direction = (targetEnemy.position - transform.position).normalized;
            transform.position += direction * deathMoveSpeed * Time.deltaTime;
            float dir = targetEnemy.position.x - transform.position.x;
            transform.localScale = new Vector3(Mathf.Sign(dir), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            animator.SetTrigger("Die");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == State.Dying && !hasHitEnemy && other.CompareTag(enemyTag))
            hasHitEnemy = true;
    }

    public void DestroySelf()
    {
        if (audioSource != null) audioSource.Stop();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (followTarget != null)
        {
            Gizmos.color = Color.green;
            Vector3 basePosition = followTarget.position + Vector3.up * flyHeight;
            Gizmos.DrawWireSphere(basePosition, flyRadius);
        }
    }
}
