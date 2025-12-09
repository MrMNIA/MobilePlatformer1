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

    // Eðer Awake'te ekstra iþlem yapacaksan:
    protected override void Awake()
    {
        base.Awake(); // Önce babanýn Awake'i çalýþsýn (rb, collider alýnsýn)
        // Sonra kendi özel kodlarýn (varsa)
        attackRangeX = boxCollider.bounds.extents.x + distance;
        attackRangeY = attackSize.y;
    }

    protected override void Update()
    {
        if (attackTimer > 0 ) attackTimer -= Time.deltaTime;
        // Babanýn Update'i çalýþsýn (Patrol/Idle mantýðý)
        base.Update();

        // BURAYA EKLEME YAPACAÐIZ:
        // Eðer oyuncu menzile girerse state = State.Chase yap
        // Chase mantýðý ve Attack vuruþunu burada override edeceðiz.
    }

    public override void Attack()
    {
        if (attackTimer <= 0)
        {
            // Sadece animasyonu baþlatýyoruz. Hasarý animasyon event verecek.
            anim.SetTrigger("meleeAttack");
            attackTimer = attackCooldown;
        }
    }

    public void DealMeleeDamage()
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // 2. BoxCast Fýrlat
        // Origin: Kendi merkezimiz (boxCollider EnemyAI'da tanýmlýydý)
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center, // Baþlangýç: Göbek deliðimiz
            attackSize,                // Boyut: Inspector'dan ayarladýðýn kutu
            0f,                        // Açý: 0 (Döndürme yok)
            Vector2.right * direction, // Yön: Baktýðýmýz yön
            distance,               // Mesafe: Ne kadar ileri?
            playerLayer                // Maske: Sadece Player'a çarp
        );

        // 3. Çarpýþma Kontrolü
        if (hit.collider != null)
        {
            // Vurduk!
            Health playerHealth = hit.collider.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Knockback uygula
                playerHealth.TakeDamage(damage, transform.position, 5f);
            }
        }


    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (boxCollider != null)
        {
            Gizmos.color = Color.red;
            float direction = transform.localScale.x > 0 ? 1f : -1f;

            // BoxCast'in varacaðý tahmini noktayý çiziyoruz
            Vector3 center = boxCollider.bounds.center + (Vector3)(Vector2.right * direction * distance);
            Gizmos.DrawWireCube(center, attackSize);
        }
    }
}
