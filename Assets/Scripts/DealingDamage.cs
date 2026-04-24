using Unity.VisualScripting;
using UnityEngine;

public class Dealingdamage : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] protected LayerMask damageLayer; // Inspector'da ayarlanacak
    [SerializeField] protected bool instaKiller = false; // Eğer true ise, hasar değeri ne olursa olsun anında öldürür.
    [SerializeField] protected float knockbackForce = 10f;
    
    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        // Layer kontrolü
        if ((damageLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            var health = collision.GetComponent<Health>();
            if (health != null)
            {
                if (instaKiller)
                {
                    health.SuddenDeath();
                }
                else
                {
                    health.TakeDamage(damage, this.transform.position, knockbackForce);
                }
            }
        }
    }


}