using Unity.VisualScripting;
using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    [SerializeField] private float healAmounth;

    [SerializeField] private AudioClip pickupSound;

    [SerializeField] private LayerMask targetLayer; // Inspector'da ayarlanacak

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Layer kontrolü
        if ((targetLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            var health = collision.GetComponent<Health>();
            if (health != null && health.GetCurrentHealthPercent() < 1f)
            {
                health.AddHealth(25f);
                SoundManager.Instance.PlaySound(pickupSound);
                gameObject.SetActive(false);
            }
        }
    }


}