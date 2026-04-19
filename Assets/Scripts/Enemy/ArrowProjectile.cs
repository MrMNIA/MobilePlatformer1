using UnityEngine;

// Dealingdamage'den türediği için damageLayer ve damage değişkenleri otomatik gelir.
public class ArrowProjectile : Dealingdamage
{
    [Header("Projectile Movement")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float resetTime = 3f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask groundLayer; // Tag yerine Layer kullanıyoruz

    private Vector2 moveDirection; // Okun gidiş yönünü tutar
    private float lifetime;
    private bool hit;

    // RangedEnemy bu fonksiyonu çağırırken Vector2 tipinde bir yön gönderecek
    public void ActivateProjectile(Vector2 _direction)
    {
        moveDirection = _direction;
        gameObject.SetActive(true);
        hit = false;
        lifetime = 0;

        // Okun ucunun gittiği yöne doğru bakması için rotasyon (Açı) hesabı
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (hit) return;

        // Okun hareketi: Yön * Hız * Zaman
        // Space.World kullanarak global eksende ilerlemesini sağlıyoruz
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
        {
            gameObject.SetActive(false);
        }
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        // 1. Dealingdamage içindeki hasar verme mantığını çalıştır (damageLayer kontrolü oradadır)
        base.OnTriggerStay2D(collision);

        // 2. LayerMask kontrolleri
        // Çarptığımız objenin layer'ı damageLayer (Player) veya groundLayer içinde mi?
        bool isTarget = ((1 << collision.gameObject.layer) & damageLayer.value) != 0;
        bool isGround = ((1 << collision.gameObject.layer) & groundLayer.value) != 0;

        if (isTarget || isGround)
        {
            hit = true;
            gameObject.SetActive(false); // Oku havuza geri gönder
        }
    }
}