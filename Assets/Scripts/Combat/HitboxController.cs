using UnityEngine;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 判定框控制器——挂攻击特效预制体上，动画事件调用 Enable/Disable。
    /// 改手感在 Animation 窗口拖 Event，不改代码。
    /// </summary>
    public class HitboxController : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField] HitboxShape _shape = HitboxShape.Box;
        [SerializeField] Vector2 _offset, _size = new(2f, 1.5f);
        [SerializeField] float _rotation, _sectorAngle = 90f;

        [Header("Damage")]
        [SerializeField] int _baseDamage = 15;
        [SerializeField] float _damageMultiplier = 1f;
        [SerializeField] DamageType _damageType;
        [SerializeField] Vector2 _knockback = new(5f, 2f);
        [SerializeField] bool _isHeavy;

        int _attackId;

        void OnEnable() { _attackId = HitTracker.BeginAttack(); }
        void OnDisable() { HitTracker.EndAttack(_attackId); HitboxManager.Instance?.EndAttack(_attackId); }

        public void EnableHitbox()
        {
            if (HitboxManager.Instance == null) return;
            float facing = transform.lossyScale.x > 0 ? 0f : 180f;
            HitboxManager.Instance.RegisterHitbox(new Hitbox
            {
                shape = _shape,
                center = (Vector2)transform.position + _offset,
                size = _shape == HitboxShape.Sector ? new Vector2(_size.x, _sectorAngle) : _size,
                rotation = facing + _rotation,
                attackId = _attackId, ownerId = gameObject.GetInstanceID(),
                damageMultiplier = _damageMultiplier, damageType = _damageType,
                baseDamage = _baseDamage, knockback = _knockback, isHeavy = _isHeavy
            });
        }

        public void DisableHitbox() { }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 pos = transform.position + (Vector3)_offset;
            float facing = transform.lossyScale.x > 0 ? 0f : 180f;
            switch (_shape)
            {
                case HitboxShape.Box:
                    Gizmos.matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, facing + _rotation), Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, _size);
                    break;
                case HitboxShape.Circle:
                    Gizmos.DrawWireSphere(pos, _size.x);
                    break;
                case HitboxShape.Sector:
                    Gizmos.DrawWireSphere(pos, _size.x);
                    break;
            }
        }
    }
}
