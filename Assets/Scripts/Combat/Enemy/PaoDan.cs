using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaoDan : MonoBehaviour
{
    public float flySpeed = 15f;
    public float lifeTime = 5f;
    private float timer;
    private Vector2 dir;
    private bool isHit = false;
    private Animator Animator;

    [Header("飞行音效（循环）")]
    public AudioClip flyLoopSound;
    public float flySoundVolume = 0.4f;
    public float soundMinDistance = 1f;
    public float soundMaxDistance = 20f;
    private AudioSource audioSource;

    void Awake()
    {
        Animator = GetComponent<Animator>();
        if (flyLoopSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.minDistance = soundMinDistance;
            audioSource.maxDistance = soundMaxDistance;
        }
    }

    void OnEnable()
    {
        timer = lifeTime;
        isHit = false;

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

    void Update()
    {
        if (timer - Time.deltaTime <= 0)
        {
            StopFlySound();
            Destroy(gameObject);
            return;
        }

        timer -= Time.deltaTime;
        transform.Translate(dir * flySpeed * Time.deltaTime);
    }

    public void Initialize(Vector2 direction)
    {
        dir = direction.normalized;

        if (dir.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            flySpeed = 0f;
            Animator.SetTrigger("break");
            StopFlySound();
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            flySpeed = 0;
            Animator.SetTrigger("break");
            StopFlySound();
        }
    }

    public void DestoryAfter()
    {
        StopFlySound();
        Destroy(gameObject);
    }
}
