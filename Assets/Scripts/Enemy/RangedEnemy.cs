using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [Header("Ranged Settings")]
    public float attackCooldown = 1.5f;
    private float attackTimer;

    [Header("Attack Settings")]
    public LayerMask playerLayer;
    public Transform firePoint;
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

    public override void Attack()
    {
        if (attackTimer <= 0)
        {
            // Sadece animasyonu ba�lat�yoruz. Hasar� animasyon event verecek.
            anim.SetTrigger("rangedAttack");
            attackTimer = attackCooldown;
        }
    }

    private void Shoot()
    {

        // Merkezi havuzdan oku çekiyoruz
        GameObject arrow = RangedArrowHolder.Instance.GetArrow();

        if (arrow != null)
        {// Okun çıkış noktası (firePoint yoksa transform.position kullanabilirsin)
            arrow.transform.position = firePoint.position;

            // Hedef - Başlangıç = Yön Vektörü
            Vector2 targetPos = new Vector2(player.position.x, (player.position.y + -0f)); // Karın boşluğuna nişan al
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            arrow.GetComponent<ArrowProjectile>().ActivateProjectile(direction);
        }
    }
}

