using Unity.VisualScripting;
using UnityEngine;

public class Dealingdamage : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] private LayerMask damageLayer; // Inspector'da ayarlanacak

    [SerializeField] private float knockbackForce = 10f;
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // Kontrol: Çarpýþan objenin Layer'ý, 'playerLayer' maskesi içinde mi?
        if ((damageLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            collision.GetComponent<Health>()?.TakeDamage(damage, transform.position, knockbackForce);
        }
    }


}