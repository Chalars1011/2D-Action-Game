using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("子弹属性")]
    public float flySpeed = 10f;
    public float trackingSpeed = 5f;
    public float lifeTime = 3f;
    public string enemyTag = "Enemy";

    [Header("飞行音效（循环）")]
    public AudioClip flyLoopSound;
    public float flySoundVolume = 0.4f;
    public float soundMinDistance = 1f;
    public float soundMaxDistance = 20f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform target;
    private bool isHit = false;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && flyLoopSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.minDistance = soundMinDistance;
            audioSource.maxDistance = soundMaxDistance;
        }
    }

    private void OnEnable()
    {
        isHit = false;
        rb.velocity = Vector2.zero;
        Invoke("DestroyBullet", lifeTime);

        if (audioSource != null && flyLoopSound != null)
        {
            audioSource.clip = flyLoopSound;
            audioSource.volume = flySoundVolume;
            audioSource.Play();
        }
    }

    private void StopFlySound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    private void FixedUpdate()
    {
        if (isHit) return;

        Vector2 currentVelocity = rb.velocity;

        if (target != null)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            Vector2 newVelocity = Vector2.Lerp(currentVelocity, directionToTarget * flySpeed, trackingSpeed * Time.fixedDeltaTime);
            rb.velocity = newVelocity;
            transform.right = newVelocity.normalized;
        }
        else
        {
            if (currentVelocity.magnitude > 0.1f)
            {
                rb.velocity = currentVelocity.normalized * flySpeed;
                transform.right = currentVelocity.normalized;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit) return;

        if (other.CompareTag(enemyTag))
        {
            isHit = true;
            rb.velocity = Vector2.zero;
            animator?.SetTrigger("Hit");
            CancelInvoke("DestroyBullet");
            StopFlySound();
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            isHit = true;
            rb.velocity = Vector2.zero;
            animator?.SetTrigger("Hit");
            CancelInvoke("DestroyBullet");
            StopFlySound();
        }
    }

    public void DestroyBullet()
    {
        StopFlySound();
        Destroy(gameObject);
    }
}
