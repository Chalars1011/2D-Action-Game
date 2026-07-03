using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HealthVignetteController : MonoBehaviour
{
    [Header("触发阈值（血量百分比）")]
    public float healthThreshold = 0.3f;

    [Header("Vignette 呼吸")]
    public float vignetteIntensityMin = 0.4f;
    public float vignetteIntensityMax = 0.6f;

    [Header("Chromatic Aberration 呼吸")]
    public float caIntensityMin = 0.15f;
    public float caIntensityMax = 0.35f;

    [Header("呼吸速度")]
    public float breatheSpeed = 1.5f;

    [Header("受伤颜色")]
    public Color hurtColor = new Color(0.8f, 0.1f, 0.05f, 1f);

    [Header("正常值")]
    public float normalVignetteIntensity = 0.35f;
    public float normalCAIntensity;
    public Color normalColor = Color.black;

    [Header("平滑过渡速度")]
    public float transitionSpeed = 3f;

    [Header("残血心跳音效")]
    public AudioClip heartbeatClip;
    [Range(0f, 1f)]
    public float heartbeatVolume = 0.5f;

    [Header("引用")]
    public Character playerCharacter;
    public Volume postProcessVolume;

    private AudioSource heartbeatSource;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private float currentVignetteIntensity;
    private float targetVignetteIntensity;
    private float currentCAIntensity;
    private float targetCAIntensity;
    private Color currentColor;
    private Color targetColor;

    void Start()
    {
        if (heartbeatClip != null)
        {
            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.clip = heartbeatClip;
            heartbeatSource.loop = true;
            heartbeatSource.playOnAwake = false;
            heartbeatSource.spatialBlend = 0f;
            heartbeatSource.volume = 0f;
        }

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out chromaticAberration);
        }

        if (vignette != null)
        {
            currentVignetteIntensity = vignette.intensity.value;
            currentColor = vignette.color.value;
        }
        if (chromaticAberration != null)
            currentCAIntensity = chromaticAberration.intensity.value;

        targetVignetteIntensity = normalVignetteIntensity;
        targetCAIntensity = normalCAIntensity;
        targetColor = normalColor;
    }

    void Update()
    {
        if (playerCharacter == null) return;

        float healthPercent = (float)playerCharacter.currentHealth / playerCharacter.maxHealth;

        if (healthPercent <= healthThreshold && healthPercent > 0)
        {
            float breathe = Mathf.Sin(Time.time * breatheSpeed) * 0.5f + 0.5f;
            targetVignetteIntensity = Mathf.Lerp(vignetteIntensityMin, vignetteIntensityMax, breathe);
            targetCAIntensity = Mathf.Lerp(caIntensityMin, caIntensityMax, breathe);
            targetColor = hurtColor;

            // 心跳音效
            if (heartbeatSource != null && !heartbeatSource.isPlaying)
            {
                heartbeatSource.Play();
                heartbeatSource.volume = heartbeatVolume;
            }
        }
        else
        {
            targetVignetteIntensity = normalVignetteIntensity;
            targetCAIntensity = normalCAIntensity;
            targetColor = normalColor;

            // 停止心跳
            if (heartbeatSource != null && heartbeatSource.isPlaying)
                heartbeatSource.Stop();
        }

        float t = Time.deltaTime * transitionSpeed;
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignetteIntensity, t);
        currentCAIntensity = Mathf.Lerp(currentCAIntensity, targetCAIntensity, t);
        currentColor = Color.Lerp(currentColor, targetColor, t);

        if (vignette != null)
        {
            vignette.intensity.value = currentVignetteIntensity;
            vignette.color.value = currentColor;
        }
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = currentCAIntensity;
    }

    void OnDisable()
    {
        if (vignette != null)
        {
            vignette.intensity.value = normalVignetteIntensity;
            vignette.color.value = normalColor;
        }
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = normalCAIntensity;
        if (heartbeatSource != null)
            heartbeatSource.Stop();
    }
}
