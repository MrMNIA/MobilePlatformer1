using Unity.VisualScripting;
using UnityEngine;

public class Dealingdamage : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] protected LayerMask damageLayer; // Inspector'da ayarlanacak

    [SerializeField] protected float knockbackForce = 10f;
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Kontrol: �arp��an objenin Layer'�, 'playerLayer' maskesi i�inde mi?
        if ((damageLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            collision.GetComponent<Health>()?.TakeDamage(damage, transform.position, knockbackForce);
        }
    }


}