using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    [Header("手动拖灯光")]
    public Light2D[] targetLights;

    [Header("波动幅度（0~1）")]
    public float intensityRange = 0.3f;

    [Header("基础变化速度")]
    public float speed = 6f;

    [Header("随机跳变强度（越大越跳）")]
    public float jitterStrength = 0.15f;

    [Header("随机跳变间隔（秒）")]
    public float jitterInterval = 0.08f;

    private float[] baseIntensities;
    private float[] baseRadii;
    private float seed;
    private Light2D[] lights;
    private float jitterTimer;
    private float currentJitter;
    private float targetJitter;

    void Start()
    {
        lights = targetLights.Length > 0 ? targetLights : GetComponents<Light2D>();
        if (lights == null || lights.Length == 0) return;

        baseIntensities = new float[lights.Length];
        baseRadii = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            baseIntensities[i] = lights[i].intensity;
            baseRadii[i] = lights[i].pointLightOuterRadius;
        }

        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (lights == null || lights.Length == 0) return;

        // 随机跳变计时
        jitterTimer -= Time.deltaTime;
        if (jitterTimer <= 0)
        {
            jitterTimer = jitterInterval + Random.Range(-jitterInterval * 0.5f, jitterInterval * 0.5f);
            targetJitter = Random.Range(-jitterStrength, jitterStrength);
        }
        currentJitter = Mathf.Lerp(currentJitter, targetJitter, Time.deltaTime * 20f);

        // 平滑噪声 + 随机跳变
        float noise = Mathf.PerlinNoise(seed, Time.time * speed);
        float mult = 1f + (noise - 0.5f) * intensityRange * 2f + currentJitter;

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = baseIntensities[i] * mult;
            lights[i].pointLightOuterRadius = baseRadii[i] * mult;
        }
    }
}
