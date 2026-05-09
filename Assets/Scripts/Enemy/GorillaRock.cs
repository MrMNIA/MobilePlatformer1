using UnityEngine;

public class GorillaRock : Dealingdamage
{
    [Header("Kaya Hareket Ayarları")]
    public float moveSpeed = 7f;
    public float rollSpeed = 180f;
    public float selfDestructTime = 5f;
    public LayerMask wallLayer;

    [Header("Efektler ve Sesler")]
    [SerializeField] private GameObject rockBreakEffectPrefab; // <--- YENİ: Kırılma efekti prefabı
    [SerializeField] private AudioClip breakSound; // <--- YENİ: Kırılma sesi (Opsiyonel)

    [Header("Referanslar")]
    [SerializeField] private Transform rockImage;
    [SerializeField] private Collider2D wallSensor;

    private Rigidbody2D rb;
    private float direction;
    private bool isExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, selfDestructTime);
    }

    // ... Start ve FixedUpdate metodları aynı kalıyor ...
    void Start()
    {
        direction = rb.linearVelocity.x > 0 ? 1f : -1f;
        if (wallSensor != null)
        {
            Vector3 pos = wallSensor.transform.localPosition;
            pos.x = Mathf.Abs(pos.x) * direction;
            wallSensor.transform.localPosition = pos;
        }
        GameObject gorilla = GameObject.FindGameObjectWithTag("Enemy");
        if (gorilla != null)
        {
            Collider2D gorilCol = gorilla.GetComponent<Collider2D>();
            foreach (var col in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(col, gorilCol);
        }
    }

    void FixedUpdate()
    {
        if (isExploded) return;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        if (rockImage != null)
            rockImage.Rotate(0, 0, -direction * rollSpeed * Time.fixedDeltaTime);
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        if (isExploded) return;

        if (wallSensor != null && wallSensor.IsTouching(collision))
        {
            if (((1 << collision.gameObject.layer) & wallLayer) != 0)
            {
                Explode();
                return;
            }
        }

        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            base.OnTriggerStay2D(collision);
            Explode();
        }
    }

    // --- EFEKT EKlenmiş EXPLODE METODU ---
    void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // Fiziği kapa

        // 1. GÖRSEL EFEKTİ OLUŞTUR
        if (rockBreakEffectPrefab != null)
        {
            // Kaya nerede patladıysa orada efekti yarat
            Instantiate(rockBreakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. SES EFEKTİNİ OYNAT (Eğer prolgende bir SoundManager varsa onu kullan, yoksa bu basit yöntemdir)
        if (breakSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(breakSound);
        }

        Debug.Log("<color=yellow>KAYA:</color> Parçalandı!");

        // Ana objeyi yok et
        Destroy(gameObject);
    }
}