using UnityEngine;

/// <summary>
/// 相机震动器——接收 ShakeProfile，用 Perlin Noise 生成自然震动。
/// 不依赖 Cinemachine，纯手动计算偏移。
/// 
/// 用法：
///   CameraShaker.Instance.Play(heavyHitProfile);
///   CameraShaker.Instance.Play(lightHitProfile);
/// </summary>
public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform _cameraTransform;

    private ShakeProfile_SO _currentProfile;
    private float _elapsed;
    private bool _isShaking;
    private Vector3 _originalPosition;

    private void Awake()
    {
        Instance = this;
        if (_cameraTransform == null)
            _cameraTransform = transform;
        _originalPosition = _cameraTransform.localPosition;
    }

    /// <summary>播放一次震动。如果正在震动中，新震动会覆盖旧震动（取更强的）。</summary>
    public void Play(ShakeProfile_SO profile)
    {
        if (profile == null) return;

        // 如果正在震动，新震动强度更高才覆盖
        if (_isShaking && _currentProfile != null)
        {
            if (profile.intensity <= _currentProfile.intensity) return;
        }

        _currentProfile = profile;
        _elapsed = 0f;
        _isShaking = true;
    }

    /// <summary>停止当前震动。</summary>
    public void Stop()
    {
        _isShaking = false;
        _currentProfile = null;
        _cameraTransform.localPosition = _originalPosition;
    }

    private void Update()
    {
        if (!_isShaking || _currentProfile == null) return;

        _elapsed += Time.deltaTime;

        if (_elapsed >= _currentProfile.duration)
        {
            // 震动结束，归位
            _cameraTransform.localPosition = _originalPosition;
            _isShaking = false;
            _currentProfile = null;
            return;
        }

        // 计算当前帧的偏移
        float t = _elapsed / _currentProfile.duration;
        float curveValue = _currentProfile.decayCurve.Evaluate(t);
        float currentIntensity = _currentProfile.intensity * curveValue;

        // Perlin Noise 产生自然随机震动（不像 Random 那样每帧跳变）
        float noiseX = (Mathf.PerlinNoise(_elapsed * _currentProfile.frequency, 0f) * 2f - 1f);
        float noiseY = (Mathf.PerlinNoise(0f, _elapsed * _currentProfile.frequency) * 2f - 1f);

        Vector3 offset = new Vector3(
            noiseX * currentIntensity * _currentProfile.directionBias.x,
            noiseY * currentIntensity * _currentProfile.directionBias.y,
            0f
        );

        _cameraTransform.localPosition = _originalPosition + offset;
    }
}
