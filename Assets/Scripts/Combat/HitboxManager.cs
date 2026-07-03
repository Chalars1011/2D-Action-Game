using System.Collections.Generic;
using UnityEngine;

namespace GameArchitecture.Combat
{
    public enum HitboxShape { Box, Circle, Sector }

    public struct Hitbox
    {
        public HitboxShape shape;
        public Vector2 center;
        public Vector2 size;
        public float rotation;
        public int attackId;
        public int ownerId;
        public float damageMultiplier;
        public DamageType damageType;
        public int baseDamage;
        public Vector2 knockback;
        public bool isHeavy;

        public bool Overlaps(Hurtbox hu)
        {
            return shape switch
            {
                HitboxShape.Box => OverlapBoxBox(center, size, rotation, hu.center, hu.size, hu.rotation),
                HitboxShape.Circle => OverlapCircleBox(center, size.x, hu.center, hu.size),
                HitboxShape.Sector => OverlapSectorBox(center, size.x, rotation, size.y, hu.center, hu.size),
                _ => false
            };
        }

        static bool OverlapBoxBox(Vector2 c1, Vector2 s1, float r1, Vector2 c2, Vector2 s2, float r2)
        {
            float dx = Mathf.Abs(c1.x - c2.x), dy = Mathf.Abs(c1.y - c2.y);
            if (dx > (s1.x + s2.x) / 2f || dy > (s1.y + s2.y) / 2f) return false;
            if (r1 == 0 && r2 == 0) return true;
            Vector2[] axes = { Rotate(Vector2.right, r1), Rotate(Vector2.up, r1),
                               Rotate(Vector2.right, r2), Rotate(Vector2.up, r2) };
            foreach (var ax in axes) { if (SATSep(c1, s1, r1, c2, s2, r2, ax)) return false; }
            return true;
        }
        static bool SATSep(Vector2 c1, Vector2 s1, float r1, Vector2 c2, Vector2 s2, float r2, Vector2 ax)
        { return ProjBox(c1, s1, r1, ax) < ProjBox(c2, s2, r2, ax); }
        static float ProjBox(Vector2 c, Vector2 s, float r, Vector2 ax)
        {
            Vector2 right = Rotate(Vector2.right, r) * s.x / 2f;
            Vector2 up = Rotate(Vector2.up, r) * s.y / 2f;
            return Vector2.Dot(c, ax) - Mathf.Abs(Vector2.Dot(right, ax)) - Mathf.Abs(Vector2.Dot(up, ax));
        }
        static bool OverlapCircleBox(Vector2 cc, float r, Vector2 bc, Vector2 bs)
        {
            float cx = Mathf.Clamp(cc.x, bc.x - bs.x / 2f, bc.x + bs.x / 2f);
            float cy = Mathf.Clamp(cc.y, bc.y - bs.y / 2f, bc.y + bs.y / 2f);
            return (cc.x - cx) * (cc.x - cx) + (cc.y - cy) * (cc.y - cy) <= r * r;
        }
        static bool OverlapSectorBox(Vector2 sc, float r, float facing, float angle, Vector2 bc, Vector2 bs)
        {
            if (!OverlapCircleBox(sc, r, bc, bs)) return false;
            Vector2[] corners = { bc + new Vector2(-bs.x/2f,-bs.y/2f), bc + new Vector2(bs.x/2f,-bs.y/2f),
                                  bc + new Vector2(-bs.x/2f,bs.y/2f), bc + new Vector2(bs.x/2f,bs.y/2f) };
            foreach (var c in corners)
            {
                Vector2 d = c - sc;
                if (d.magnitude <= r && Vector2.Angle(Rotate(Vector2.right, facing), d) <= angle / 2f) return true;
            }
            return false;
        }
        static Vector2 Rotate(Vector2 v, float deg)
        {
            float rad = deg * Mathf.Deg2Rad;
            return new Vector2(v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad), v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad));
        }
    }

    public struct Hurtbox
    {
        public Vector2 center, size;
        public float rotation;
        public int ownerId;
        public HurtboxType type;
        public float damageMultiplier;
    }

    public enum HurtboxType { Body, Head, WeakPoint, Armor }

    /// <summary>
    /// 帧驱动判定引擎。每帧 LateUpdate 执行一次。
    /// 替代物理 OnTriggerEnter2D，实现精确帧级判定。
    /// </summary>
    public class HitboxManager : MonoBehaviour
    {
        public static HitboxManager Instance { get; private set; }
        readonly List<Hitbox> _hb = new();
        readonly List<Hurtbox> _hu = new();
        readonly HashSet<(int, int)> _reg = new();

        void Awake() { Instance = this; }
        public void RegisterHitbox(Hitbox h) { _hb.Add(h); }
        public void RegisterHurtbox(Hurtbox h) { _hu.Add(h); }

        void LateUpdate()
        {
            if (_hb.Count == 0) { Clear(); return; }
            for (int i = 0; i < _hb.Count; i++)
            {
                var h = _hb[i];
                for (int j = 0; j < _hu.Count; j++)
                {
                    var u = _hu[j];
                    if (h.ownerId == u.ownerId) continue;
                    var key = (h.attackId, u.ownerId);
                    if (_reg.Contains(key)) continue;
                    if (h.Overlaps(u))
                    {
                        _reg.Add(key);
                        var target = FindTarget(u.ownerId);
                        if (target != null && target.IsAlive)
                            target.TakeDamage(new DamageInput
                            {
                                baseDamage = h.baseDamage * h.damageMultiplier * u.damageMultiplier,
                                damageType = h.damageType, attackerInstanceId = h.ownerId,
                                targetInstanceId = u.ownerId, isHeavyHit = h.isHeavy,
                                knockbackDirection = h.knockback
                            });
                    }
                }
            }
            Clear();
        }

        void Clear() { _hb.Clear(); _hu.Clear(); }

        public void EndAttack(int id)
        {
            List<(int, int)> rem = new();
            foreach (var k in _reg) if (k.Item1 == id) rem.Add(k);
            foreach (var k in rem) _reg.Remove(k);
        }

        HealthComponent FindTarget(int id)
        {
            var all = FindObjectsOfType<HealthComponent>();
            foreach (var h in all) if (h.GetInstanceID() == id) return h;

            // 兼容旧 Character 组件
            var chars = FindObjectsOfType<Character>();
            foreach (var c in chars)
            {
                if (c.GetInstanceID() == id)
                {
                    c.currentHealth -= 10; // 临时占位伤害，走旧 TakeDmage 路径
                    return null;
                }
            }
            return null;
        }
    }
}
