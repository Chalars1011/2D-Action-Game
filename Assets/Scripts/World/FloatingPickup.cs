using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FloatingPickup : MonoBehaviour
{
    [Header("上下浮动")]
    public float floatAmplitude = 0.3f;
    public float floatSpeed = 1.5f;

    [Header("呼吸灯光")]
    public Light2D pickupLight;
    public float lightIntensityMin = 0.3f;
    public float lightIntensityMax = 1.2f;
    public float lightRadiusMin = 1.5f;
    public float lightRadiusMax = 3f;
    public float breatheSpeed = 2f;

    private Vector3 startPos;
    private float timeOffset;

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // 上下浮动
        float y = Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatAmplitude;
        transform.position = startPos + new Vector3(0, y, 0);

        // 呼吸灯
        if (pickupLight != null)
        {
            float breathe = Mathf.Sin(Time.time * breatheSpeed + timeOffset) * 0.5f + 0.5f;
            pickupLight.intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, breathe);
            pickupLight.pointLightOuterRadius = Mathf.Lerp(lightRadiusMin, lightRadiusMax, breathe);
        }
    }
}
