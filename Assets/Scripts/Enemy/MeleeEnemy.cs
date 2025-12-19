using UnityEngine;

public class MeleeEnemy : EnemyAI
{
    [Header("Melee Settings")]
    public float damage = 10f;
    public float attackCooldown = 1f;
    private float attackTimer;

    [Header("Attack Settings")]
    public Vector2 attackSize = new Vector2(2f, 2f);
    public float distance = 1.5f;
    public LayerMask playerLayer;

    // E�er Awake'te ekstra i�lem yapacaksan:
    protected override void Awake()
    {
        base.Awake(); // �nce baban�n Awake'i �al��s�n (rb, collider al�ns�n)
        // Sonra kendi �zel kodlar�n (varsa)
        attackRangeX = boxCollider.bounds.extents.x + distance;
        attackRangeY = attackSize.y;
    }

    protected override void Update()
    {
        if (attackTimer > 0 ) attackTimer -= Time.deltaTime;
        // Baban�n Update'i �al��s�n (Patrol/Idle mant���)
        base.Update();

        // BURAYA EKLEME YAPACA�IZ:
        // E�er oyuncu menzile girerse state = State.Chase yap
        // Chase mant��� ve Attack vuru�unu burada override edece�iz.
    }

    public override void Attack()
    {
        if (attackTimer <= 0)
        {
            // Sadece animasyonu ba�lat�yoruz. Hasar� animasyon event verecek.
            anim.SetTrigger("meleeAttack");
            attackTimer = attackCooldown;
        }
    }

    public void DealMeleeDamage()
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // 2. BoxCast F�rlat
        // Origin: Kendi merkezimiz (boxCollider EnemyAI'da tan�ml�yd�)
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center, // Ba�lang��: G�bek deli�imiz
            attackSize,                // Boyut: Inspector'dan ayarlad���n kutu
            0f,                        // A��: 0 (D�nd�rme yok)
            Vector2.right * direction, // Y�n: Bakt���m�z y�n
            distance,               // Mesafe: Ne kadar ileri?
            playerLayer                // Maske: Sadece Player'a �arp
        );

        // 3. �arp��ma Kontrol�
        if (hit.collider != null)
        {
            // Vurduk!
            Health playerHealth = hit.collider.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Knockback uygula
                playerHealth.TakeDamage(damage, transform.position, 6f);
            }
        }


    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        float OffsetX = (boxCollider != null) ? boxCollider.bounds.extents.x + 0.1f : 0.5f;
        if (!movingRight) OffsetX *= -1;
        Vector3 rayOrigin = transform.position + new Vector3(OffsetX, -0.5f, 0);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayOrigin, Vector2.down * rayDistance);
    }
}
