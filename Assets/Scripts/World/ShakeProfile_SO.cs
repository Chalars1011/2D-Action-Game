using UnityEngine;

/// <summary>
/// 屏幕震动配置文件——定义一次震动的完整参数。
/// 在 Inspector 里可视化编辑衰减曲线，不需要改代码。
/// 
/// 用法：
///   1. 右键 Create → Camera → Shake Profile 创建资产
///   2. 调整强度/时长/曲线
///   3. 代码: CameraShaker.Play(profile)
/// </summary>
[CreateAssetMenu(menuName = "Camera/Shake Profile")]
public class ShakeProfile_SO : ScriptableObject
{
    [Header("Intensity")]
    [Tooltip("最大偏移量（世界单位）")]
    [Range(0f, 5f)]
    public float intensity = 0.5f;

    [Header("Duration")]
    [Tooltip("震动持续秒数")]
    [Range(0.01f, 2f)]
    public float duration = 0.15f;

    [Header("Frequency")]
    [Tooltip("震动频率。值越高抖动越快")]
    [Range(10f, 120f)]
    public float frequency = 50f;

    [Header("Decay Curve")]
    [Tooltip("强度随时间衰减的曲线。X轴=时间比例(0-1)，Y轴=强度比例(0-1)")]
    public AnimationCurve decayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Direction Bias")]
    [Tooltip("震动偏向：x=水平偏向，y=垂直偏向。(1,1)=全方向，(1,0)=只水平震")]
    public Vector2 directionBias = Vector2.one;

    private void OnValidate()
    {
        intensity = Mathf.Max(0, intensity);
        duration = Mathf.Max(0.01f, duration);
        frequency = Mathf.Max(1f, frequency);
    }
}
