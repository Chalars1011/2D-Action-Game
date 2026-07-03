using UnityEngine;

public class TrapAttack : AttackBase
{
    [Header("陷阱伤害冷却")]
    public float damageCooldown = 0.5f;
    private float cooldownTimer;

    private void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDamage(collision);
    }

    private void TryDamage(Collider2D collision)
    {
        if (cooldownTimer > 0) return;

        Character character = collision.GetComponent<Character>();
        if (character == null || character.currentHealth <= 0) return;

        PlayerController playerCtrl = collision.GetComponent<PlayerController>();
        if (playerCtrl != null && playerCtrl.isHurt) return;

        cooldownTimer = damageCooldown;
        AttackTotalEffect(collision);
    }
}
