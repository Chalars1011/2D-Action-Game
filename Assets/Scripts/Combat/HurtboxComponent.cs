using UnityEngine;

namespace GameArchitecture.Combat
{
    /// <summary>
    /// 受击框组件——挂在任何"会被打到的东西"上。
    /// 每帧自动把自己的碰撞体形状注册到 HitboxManager。
    /// 
    /// 玩家、敌人、可破坏物、Boss 都挂这个。
    /// 支持多个 Hurtbox（头部、身体、弱点分别定义）
    /// </summary>
    public class HurtboxComponent : MonoBehaviour
    {
        [Header("Hurtbox")]
        [SerializeField] private Vector2 _size = new(1f, 1.5f);
        [SerializeField] private Vector2 _offset;
        [SerializeField] private HurtboxType _type = HurtboxType.Body;
        [SerializeField] private float _damageMultiplier = 1f;

        private void OnEnable()
        {
            if (HitboxManager.Instance != null)
                HitboxManager.Instance.RegisterHurtbox(new Hurtbox
                {
                    center = (Vector2)transform.position + _offset,
                    size = _size,
                    rotation = 0,
                    ownerId = gameObject.GetInstanceID(),
                    type = _type,
                    damageMultiplier = _damageMultiplier
                });
        }

        /// <summary>
        /// 每帧注册（Hurtbox 跟随实体移动）。
        /// HitboxManager 在 LateUpdate 清空，所以每帧都要注册。
        /// 如果实体不动（如陷阱），可以不挂此组件，手动注册一次即可。
        /// </summary>
        private void Update()
        {
            if (HitboxManager.Instance != null)
                HitboxManager.Instance.RegisterHurtbox(new Hurtbox
                {
                    center = (Vector2)transform.position + _offset,
                    size = _size,
                    rotation = 0,
                    ownerId = gameObject.GetInstanceID(),
                    type = _type,
                    damageMultiplier = _damageMultiplier
                });
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireCube(transform.position + (Vector3)_offset, _size);
        }
    }
}
