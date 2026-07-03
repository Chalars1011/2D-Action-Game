using UnityEngine;
using GameArchitecture.Core;
using Cinemachine;

/// <summary>
/// 相机预瞄——创建一个虚拟跟随点偏向前方，让 Cinemachine 跟随它。
/// 不直接动相机位置（会被 Cinemachine 覆盖），而是动 Follow 目标。
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
[DefaultExecutionOrder(100)] // 确保在 CameraFollowPlayer(默认0)之后执行
public class CameraLookAhead : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _lookAheadDistance = 2f;
    [SerializeField] private float _smoothTime = 0.3f;

    private CinemachineVirtualCamera _vCam;
    private Transform _lookTarget;
    private float _currentOffsetX;
    private float _velocityX;

    private void Awake()
    {
        _vCam = GetComponent<CinemachineVirtualCamera>();

        // 创建虚拟跟随点（挂在相机下，不影响 Hierarchy）
        var go = new GameObject("_LookAheadTarget");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        _lookTarget = go.transform;
    }

    private void Start()
    {
        if (_playerTransform == null)
            _playerTransform = Blackboard.PlayerTransform;

        // 用虚拟点替换 Cinemachine 的 Follow 目标
        if (_vCam != null && _lookTarget != null)
            _vCam.Follow = _lookTarget;
    }

    private void Update()
    {
        if (_playerTransform == null || _lookTarget == null) return;

        // 虚拟点 = 玩家位置 + 预瞄偏移
        float facing = _playerTransform.localScale.x > 0 ? 1f : -1f;
        float target = facing * _lookAheadDistance;
        _currentOffsetX = Mathf.SmoothDamp(_currentOffsetX, target, ref _velocityX, _smoothTime);

        _lookTarget.position = new Vector3(
            _playerTransform.position.x + _currentOffsetX,
            _playerTransform.position.y,
            _playerTransform.position.z
        );
    }

    private void OnDestroy()
    {
        if (_lookTarget != null)
            Destroy(_lookTarget.gameObject);
    }
}
