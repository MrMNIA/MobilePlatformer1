using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [Header("Ranged Settings")]
    public float attackCooldown = 1.5f;
    private float attackTimer;

    [Header("Attack Settings")]
    public LayerMask playerLayer;
    public Transform firePoint;

    private Vector2 lockedDirection; // Saldırı başladığında yönü burada saklayacağız

    // E�er Awake'te ekstra i�lem yapacaksan:
    protected override void Awake()
    {
        base.Awake();
        
    }

    protected override void Update()
    {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        // Baban�n Update'i �al��s�n (Patrol/Idle mant���)
        base.Update();
    }

    protected override bool ReadyToAttack()
    {
        // Hem isAttacking false olmalı (animasyon bitmiş olmalı)
        // Hem de senin cooldown sayacın (attackTimer) sıfırlanmış olmalı
        return base.ReadyToAttack() && attackTimer <= 0;
    }


    public override void Attack()
    {
        if (attackTimer <= 0)
        {
            base.Attack();

            // --- YÖNÜ BURADA HESAPLIYOR VE KİLİTLİYORUZ ---
            Vector2 targetPos = new Vector2(player.position.x, player.position.y);
            lockedDirection = (targetPos - (Vector2)transform.position).normalized;
            // ----------------------------------------------

            anim.SetTrigger("rangedAttack");
            attackTimer = attackCooldown;
        }
    }

    private void Shoot()
    {
        GameObject arrow = RangedArrowHolder.Instance.GetArrow();

        if (arrow != null)
        {
            arrow.transform.position = firePoint.position;

            // Hesaplanan değil, önceden kaydedilen (locked) yönü kullanıyoruz
            arrow.GetComponent<ArrowProjectile>().ActivateProjectile(lockedDirection);
        }
    }
}

