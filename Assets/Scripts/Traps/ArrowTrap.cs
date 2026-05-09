using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float attackCooldown = 2f; // Ateşleme aralığı
    [SerializeField] private float initialDelay = 0f;  // Oyun başladığında ilk ateşleme için beklenecek süre
    [SerializeField] private Transform firePoint;     // Okun çıkacağı nokta

    private float cooldownTimer;

    private void Start()
    {
        // Cooldown timer'ı başlangıç gecikmesine göre ayarla.
        // attackCooldown'dan initialDelay'i çıkarıyoruz ki;
        // initialDelay 0 ise hemen ateş etsin, 5 ise 5 saniye bekleyip ateş etsin.
        cooldownTimer = attackCooldown - initialDelay;
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= attackCooldown)
        {
            Attack();
        }
    }

    private void Attack()
    {
        cooldownTimer = 0;

        // 1. Havuzdan bir ok al
        GameObject arrow = RangedArrowHolder.Instance.GetArrow();

        if (arrow != null)
        {
            // 2. Okun pozisyonunu borunun ağzına getir
            arrow.transform.position = firePoint.position;

            // 3. Oku fırlat (Borunun baktığı yöne doğru)
            arrow.GetComponent<ArrowProjectile>().ActivateProjectile(transform.right);
        }
    }
}