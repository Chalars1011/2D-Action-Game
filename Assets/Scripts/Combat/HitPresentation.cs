using System;
using UnityEngine;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 命中表现层——震屏、音效、血特效、时间冻结。
    /// 与 HitboxComponent 完全解耦，通过订阅 HitLandedEvent 驱动。
    /// 
    /// 设计原则：
    ///   1. 只挂到玩家攻击预制体上（敌人攻击不需要震屏）
    ///   2. 通过 EventBus 订阅 HitLandedEvent，不与 HitboxComponent 直接引用
    ///   3. 所有参数可配置，不硬编码
    /// </summary>
    public class HitPresentation : MonoBehaviour
    {
        [Header("Screen Shake")]
        [SerializeField] private bool _enableShake = true;
        [Tooltip("优先使用 ShakeProfile 资产；为空则用下面的硬编码参数")]
        [SerializeField] private ShakeProfile_SO _shakeProfile;
        [SerializeField] private float _shakeIntensity = 0.3f;
        [SerializeField] private float _shakeDuration = 0.15f;
        [SerializeField] private float _shakeFrequency = 30f;

        [Header("Time Freeze")]
        [SerializeField] private bool _enableFreeze = false;
        [SerializeField] private float _freezeDuration = 0.05f;
        [SerializeField] private float _freezeIntensity = 0f;

        [Header("Hit Sound")]
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private float _hitSoundVolume = 0.8f;

        [Header("Blood Effect")]
        [SerializeField] private bool _enableBloodEffect = true;

        private IDisposable _hitSubscription;

        // 静态查找表：InstanceID → Transform，避免每帧 FindObjectsOfType
        private static readonly System.Collections.Generic.Dictionary<int, Transform> _targetCache = new();

        private void OnEnable()
        {
            _hitSubscription = EventBus.On<HitLandedEvent>(OnHitLanded);
        }

        private void OnDisable()
        {
            _hitSubscription?.Dispose();
        }

        /// <summary>HealthComponent.Awake 调用此方法注册目标。</summary>
        public static void RegisterTarget(int instanceId, Transform target)
        {
            _targetCache[instanceId] = target;
        }

        /// <summary>HealthComponent.OnDestroy 调用此方法注销。</summary>
        public static void UnregisterTarget(int instanceId)
        {
            _targetCache.Remove(instanceId);
        }

        private void OnHitLanded(HitLandedEvent evt)
        {
            if (evt.attackerId != GetInstanceID()
                && evt.attackerId != transform.parent?.GetInstanceID())
                return;

            if (_enableShake)
            {
                if (_shakeProfile != null && CameraShaker.Instance != null)
                    CameraShaker.Instance.Play(_shakeProfile);
                else if (EffectPoolManager.Instance != null)
                    EffectPoolManager.Instance.ShakeScreen(_shakeIntensity, _shakeDuration, _shakeFrequency);
            }

            if (_enableFreeze && EffectPoolManager.Instance != null)
                EffectPoolManager.Instance.FreezeTime(_freezeDuration, _freezeIntensity);

            if (_hitSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayAtPoint(_hitSound, evt.contactPoint, _hitSoundVolume, AudioCategory.Hit);

            // O(1) 查找，不是 O(n) 全场景扫描
            if (_enableBloodEffect && EffectPoolManager.Instance != null
                && _targetCache.TryGetValue(evt.targetId, out var targetTransform))
            {
                var bloodTransform = targetTransform.GetComponent<Character>()?.BloodTransform;
                if (bloodTransform != null)
                {
                    Vector2 dir = (targetTransform.position - transform.position).normalized;
                    int dirVal = dir.x < -0.1f ? -1 : (dir.x > 0.1f ? 1 : 0);
                    EffectPoolManager.Instance.BloodEffect_1(bloodTransform, dirVal);
                }
            }
        }
    }
}
