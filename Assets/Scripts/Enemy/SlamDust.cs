using UnityEngine;

public class SlamDust : Dealingdamage
{
    public float lifetime = 0.6f;

    // Tüm SlamDust objeleri arasında paylaşılan bir "son hasar zamanı"
    private static float lastHitTime = -4f;
    private float hitCooldown = 4f;

    void Start()
    {
        GetComponentInChildren<ParticleSystem>().Play();
        Destroy(gameObject, lifetime);
    }

    // Dealingdamage içindeki hasar mantığını override ediyoruz
    protected override void OnTriggerStay2D(Collider2D collision)
    {
        // Sadece Player katmanına (damageLayer) çarpıp çarpmadığımızı kontrol et
        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            // Eğer son vuruşun üzerinden 4 saniye geçmemişse, hasar verme
            if (Time.time < lastHitTime + hitCooldown)
            {
                return;
            }

            // Eğer kod buraya geldiyse hasar verebiliriz
            var health = collision.GetComponent<Health>();
            if (health != null && !health.isDead)
            {
                // Zamanlayıcıyı güncelle (Bu tüm toz bulutları için ortak çalışır)
                lastHitTime = Time.time;

                // Üst sınıftaki (Dealingdamage) hasar verme fonksiyonunu çağır
                base.OnTriggerStay2D(collision);

                Debug.Log("<color=orange>DEPREM:</color> Oyuncu hasar aldı, 4 saniye dokunulmazlık başladı.");
            }
        }
    }
}