using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[DefaultExecutionOrder(500)]
public class EffectPoolManager : Singleton<EffectPoolManager>
{
    [Header("时间冻结参数")]
    public float defaultFreezeDuration = 0.08f;
    public float defaultTimeScale = 0f;

    [Header("屏幕震动参数")]
    public float defaultShakeIntensity = 0.4f;
    public float defaultShakeDuration = 0.25f;
    public float defaultShakeFrequency = 25f;

    [Header("摄像机引用")]
    public CinemachineVirtualCamera virtualCamera;

    private Camera mainCam;
    private float shakeIntensity;
    private float shakeDuration;
    private float shakeFrequency;
    private float shakeTimer;
    private float shakeSeed;

    // 持续底震（独立于打击瞬震）
    private bool ambientShakeActive;
    private float ambientShakeIntensity;
    private float ambientShakeFrequency;

    private Coroutine timeEffectCoroutine;
    private float savedTimeScale = 1f;

    void Start()
    {
        savedTimeScale = Time.timeScale;
        shakeSeed = Random.Range(0f, 100f);
        mainCam = Camera.main;
    }

    // ==================== 打击瞬震 ====================

    public void ShakeScreen(float intensity = -1f, float duration = -1f, float frequency = -1f)
    {
        if (intensity <= 0) intensity = defaultShakeIntensity;
        if (duration <= 0) duration = defaultShakeDuration;
        if (frequency <= 0) frequency = defaultShakeFrequency;

        if (intensity > shakeIntensity) shakeIntensity = intensity;
        if (duration > shakeTimer)
        {
            shakeTimer = duration;
            shakeDuration = duration;
        }
        shakeFrequency = frequency;
    }

    // ==================== 持续底震 ====================

    public void StartAmbientShake(float intensity, float frequency)
    {
        ambientShakeActive = true;
        ambientShakeIntensity = intensity;
        ambientShakeFrequency = frequency;
    }

    public void StopAmbientShake()
    {
        ambientShakeActive = false;
    }

    void LateUpdate()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // 计算瞬震强度
        float burstIntensity = 0f;
        float burstFreq = shakeFrequency;
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            float decay = shakeTimer / shakeDuration;
            burstIntensity = shakeIntensity * decay;
        }
        else
        {
            shakeIntensity = 0f;
        }

        // 取瞬震和底震的较大值
        float finalIntensity = burstIntensity;
        float finalFreq = burstFreq;

        if (ambientShakeActive && ambientShakeIntensity > finalIntensity)
        {
            finalIntensity = ambientShakeIntensity;
            finalFreq = ambientShakeFrequency;
        }

        if (finalIntensity > 0.001f)
        {
            float x = (Mathf.PerlinNoise(shakeSeed, Time.unscaledTime * finalFreq) * 2f - 1f) * finalIntensity;
            float y = (Mathf.PerlinNoise(Time.unscaledTime * finalFreq, shakeSeed) * 2f - 1f) * finalIntensity;
            mainCam.transform.position += new Vector3(x, y, 0f);
        }
    }

    // ==================== 时间冻结 ====================

    public void FreezeTime(float duration = -1f, float timeScale = -1f)
    {
        if (duration <= 0) duration = defaultFreezeDuration;
        if (timeScale < 0) timeScale = defaultTimeScale;

        // 只在第一次冻结时保存原始流速
        if (timeEffectCoroutine == null)
            savedTimeScale = Time.timeScale;

        // 刷新计时——后到的冻结延长停留时间
        if (timeEffectCoroutine != null)
            StopCoroutine(timeEffectCoroutine);

        Time.timeScale = timeScale;
        timeEffectCoroutine = StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = savedTimeScale;
        timeEffectCoroutine = null;
    }

    // ==================== 停止所有效果 ====================

    public void StopAllEffects()
    {
        shakeTimer = 0f;
        shakeIntensity = 0f;

        if (timeEffectCoroutine != null)
        {
            StopCoroutine(timeEffectCoroutine);
            Time.timeScale = savedTimeScale;
            timeEffectCoroutine = null;
        }
    }

    // ==================== 特效生成 ====================

    public void ChongCi_Start_Effect(Transform t, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("ChongCi_Start_Effect");
        e.transform.position = t.position + new Vector3(0, 0.25f, 0);
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void ChongCi_Stop_Effect(Transform t, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("ChongCi_Stop_Effect");
        e.transform.position = t.position + new Vector3(0, 0.5f, 0);
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void RunningEffect(Transform t, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("RunningEffect");
        e.transform.position = t.position;
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void RunStopEffect(Transform t, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("RunStopEffect");
        e.transform.position = t.position;
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void BloodEffect_1(Transform t, int faceDir)
    {
        if (t == null) return;
        GameObject e = PoolManager.Instance.GetObj("BloodEffect_1");
        e.transform.position = t.position;
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void BloodEffect_2(Transform t, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("BloodEffect_2");
        e.transform.position = t.position;
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void BloodEffect_3(Transform t, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("BloodEffect_3");
        e.transform.position = t.position;
        e.transform.localScale = new Vector3(faceDir, t.localScale.y, t.localScale.z);
    }

    public void Light_midleLight(Vector3 position, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("MidleLight");
        e.transform.position = position;
        e.transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);
    }

    public void Light_long(Vector3 position, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("LongLight");
        e.transform.position = position;
        e.transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);
    }

    public void Light_fly(Vector3 position, int faceDir)
    {
        GameObject e = PoolManager.Instance.GetObj("LightFly");
        e.transform.position = position;
        e.transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);
    }
}
