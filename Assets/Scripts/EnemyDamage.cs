using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] private LayerMask playerLayer; // Inspector'da ayarlanacak
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // Kontrol: Çarpýþan objenin Layer'ý, 'playerLayer' maskesi içinde mi?
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            collision.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}