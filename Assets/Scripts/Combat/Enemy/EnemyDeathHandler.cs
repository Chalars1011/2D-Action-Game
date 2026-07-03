using UnityEngine;
using GameArchitecture.Core;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 敌人死亡处理器——掉落金币、播放死亡动画。
    /// 通过订阅 HealthComponent.OnDied 事件驱动，不再轮询 currentHealth。
    /// 
    /// 为什么独立？
    ///   旧的 EnemyBase 在 Update 里每帧检查 currentHealth <= 0（轮询）。
    ///   但 HealthComponent.OnDied 已经是事件驱动的了——不用轮询。
    ///   把死亡逻辑抽出来，EnemyBase 只负责 AI（巡逻/追击/攻击）。
    /// </summary>
    public class EnemyDeathHandler : MonoBehaviour
    {
        [Header("Coin Drop")]
        [SerializeField] private int _coinDropAmount = 3;
        [SerializeField] private float _coinSpawnRadius = 1.5f;
        [SerializeField] private float _coinSpawnRandomness = 0.3f;
        [SerializeField] private Transform _coinSpawnPoint;

        [Header("Death Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _deathBoolName = "dead";

        [Header("Cleanup")]
        [SerializeField] private Collider2D[] _collidersToDisable;
        [SerializeField] private Rigidbody2D _rb;

        private HealthComponent _health;
        private bool _isDead;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();

            if (_health != null)
            {
                _health.OnDied.AddListener(HandleDeath);
                _health.OnFatalHit.AddListener(_ => HandleDeath());
            }
        }

        private void HandleDeath()
        {
            if (_isDead) return;
            _isDead = true;

            // 死亡动画
            if (_animator != null)
                _animator.SetBool(_deathBoolName, true);

            // 禁用碰撞体
            if (_collidersToDisable != null)
            {
                foreach (var col in _collidersToDisable)
                    if (col != null) col.enabled = false;
            }
            else
            {
                foreach (var col in GetComponents<Collider2D>())
                    col.enabled = false;
            }

            // 停止物理
            if (_rb != null)
            {
                _rb.gravityScale = 0;
                _rb.velocity = Vector2.zero;
            }

            // 掉落金币
            SpawnCoins();

            // 切换层（避免后续交互）
            gameObject.layer = 2; // IgnoreRaycast
        }

        private void SpawnCoins()
        {
            if (_coinDropAmount <= 0) return;

            Vector3 spawnCenter = _coinSpawnPoint != null
                ? _coinSpawnPoint.position
                : transform.position;

            float angleStep = 360f / _coinDropAmount;

            for (int i = 0; i < _coinDropAmount; i++)
            {
                float baseAngle = i * angleStep;
                float randomOffset = Random.Range(-_coinSpawnRandomness * angleStep / 2,
                                                  _coinSpawnRandomness * angleStep / 2);
                float finalAngle = baseAngle + randomOffset;
                float angleRad = finalAngle * Mathf.Deg2Rad;
                float radius = _coinSpawnRadius * Random.Range(0.7f, 1.0f);

                Vector3 spawnPos = spawnCenter + new Vector3(
                    Mathf.Cos(angleRad) * radius,
                    Mathf.Sin(angleRad) * radius,
                    0);

                if (PoolManager.Instance != null)
                {
                    GameObject coin = PoolManager.Instance.GetObj("LingHun");
                    if (coin != null)
                        coin.transform.position = spawnPos;
                }
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDied.RemoveListener(HandleDeath);
                _health.OnFatalHit.RemoveListener(_ => HandleDeath());
            }
        }
    }
}
