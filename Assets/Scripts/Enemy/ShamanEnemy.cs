using UnityEngine;
using System.Collections.Generic; // Listeler için gerekli

public class ShamanEnemy : EnemyAI
{
    [Header("Shaman - Attack Settings")]
    public float attackCooldown = 2f;
    private float attackTimer;
    public Transform firePoint;

    [Header("Shaman - Heal Settings")]
    public float healCooldown = 6f;
    private float healTimer;
    public float healRange = 8f;
    public int healAmount = 20;
    public LayerMask enemyLayer; // Dostları bulmak için kullanılacak
    public GameObject healEffectPrefab; // İyileştirme anında çıkacak görsel

    public bool isHealing = false; // İyileştirme yaparken hareket etmesini engellemek için

    private Health currentHealTarget = null; // İyileştirilecek hedefi geçici olarak burada tutuyoruz

    protected override void Awake()
    {
        base.Awake();
        // Başlangıçta hemen iyileştirme yapmasın diye süreyi başlatabiliriz
        healTimer = healCooldown;
    }

    protected override void Update()
    {
        // Zamanlayıcıları düşür
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (healTimer > 0) healTimer -= Time.deltaTime;

        // Eğer iyileştirme yapıyorsa, hareketi durdur ve base.Update'in çalışmasını engelle
        if (isHealing)
        {
            rib.linearVelocity = new Vector2(0, rib.linearVelocity.y);
            return;
        }

        // İyileştirme süresi geldiyse ve saldırı yapmıyorsa etrafı kontrol et
        if (state != State.Stasis && healTimer <= 0 && !isAttacking)
        {
            if (TryHealAlly())
            {
                // Eğer iyileştirilecek biri bulunduysa ve iyileştirme başladıysa, 
                // bu frame'de hareket/saldırı kodlarının (base.Update) çalışmaması için return diyoruz.
                return;
            }
        }

        // Babanın Update'i çalışsın (Patrol/Idle/Chase/Attack kararları)
        base.Update();
    }

    protected override bool ReadyToAttack()
    {
        // Hem isAttacking false olmalı (animasyon bitmiş olmalı)
        // Hem de senin cooldown sayacın (attackTimer) sıfırlanmış olmalı
        return base.ReadyToAttack() && attackTimer <= 0;
    }

    // --- SALDIRI MANTIĞI ---
    public override void Attack()
    {
        if (attackTimer <= 0)
        {
            base.Attack(); // isAttacking = true yapar

            // Sadece animasyonu başlatıyoruz. Tıpkı RangedEnemy'deki gibi mermiyi animasyon event atacak.
            anim.SetTrigger("rangedAttack"); // veya shaman için ayrı bir tetikleyici: "shamanAttack"
            attackTimer = attackCooldown;
        }
    }

    // Bu metodu animasyon event'i ile çağır (Ateş topunun atılma anında)
    private void Shoot()
    {
        // Merkezi havuzdan ateş topunu çekiyoruz (Senin RangedArrowHolder mantığınla aynı)
        GameObject fireball = ShamanFireballHolder.Instance.GetFireball();

        if (fireball != null)
        {
            fireball.transform.position = firePoint.position;

            // Hedef yönü belirle (Göğüs/Karın hizasına atması için player.position.y ayarı)
            Vector2 targetPos = new Vector2(player.position.x, player.position.y);
            Vector2 direction = (targetPos - (Vector2)firePoint.position).normalized;

            // Ateş topunu aktifleştir
            fireball.GetComponent<FireballProjectile>().ActivateProjectile(direction);
        }
    }

    // --- İYİLEŞTİRME MANTIĞI ---
    private bool TryHealAlly()
    {
        // Etraftaki "Enemy" katmanındaki objeleri bul
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, healRange, enemyLayer);

        Health targetToHeal = null;
        float minHealthPercent = 100f; // Canı en az olanı bulmak için (Eğer % değeri 0-1 arasıysa bunu 1f yap)

        foreach (var ally in allies)
        {
            // Kendini iyileştirmesini istemiyorsan:
            if (ally.gameObject == gameObject) continue;

            Health allyHealth = ally.GetComponent<Health>();
            if (allyHealth != null)
            {
                // Sağlık yüzdesini al
                float hpPercent = allyHealth.GetCurrentHealthPercent();

                // Eğer canı tam değilse ve şu ana kadar bulduğumuz en düşük can yüzdesinden daha düşükse
                // Not: Eğer GetCurrentHealthPercent 0.0 - 1.0 arası değer döndürüyorsa hpPercent < 1f kullanmalısın.
                // Eğer 0 - 100 arası döndürüyorsa hpPercent < 100f kullanmalısın.
                if (hpPercent < 1f && hpPercent < minHealthPercent) // < 1f olduğunu varsayıyorum (1 = %100)
                {
                    minHealthPercent = hpPercent;
                    targetToHeal = allyHealth;
                }
            }
        }

        // Eğer canı azalmış bir dost bulduysak
        if (targetToHeal != null)
        {
            StartHealing(targetToHeal);
            return true;
        }

        return false;
    }


    private void StartHealing(Health target)
    {
        isHealing = true;
        currentHealTarget = target; // Hedefi kaydet

        anim.SetBool("isRunning", false);
        rib.linearVelocity = Vector2.zero; // Hareket durdur

        anim.SetTrigger("heal"); // İyileştirme animasyonunu başlat

        healTimer = healCooldown; // Cooldown'ı burada başlatıyoruz ki tekrar tekrar tetiklenmesin
    }

    // --- BU METODU ANİMASYONUN TAM "O" Karesine EKLE ---
    public void ExecuteHeal()
    {
        // Eğer animasyon oynarken hedef öldüyse veya yok olduysa hata almamak için kontrol
        if (currentHealTarget != null)
        {
            // 1. Gerçek iyileştirme işlemi
            currentHealTarget.AddHealth(healAmount);

            // 2. Görsel efekt oluşturma
            if (healEffectPrefab != null)
            {
                // Efekti hedefin altına (child olarak) oluşturuyoruz
                Instantiate(healEffectPrefab, currentHealTarget.transform.position, Quaternion.identity, currentHealTarget.transform);
            }

            Debug.Log(currentHealTarget.gameObject.name + " başarıyla iyileştirildi!");
        }

        // İşlem bittiği için hedefi temizle (opsiyonel)
        currentHealTarget = null;
    }

    // Bu metod animasyonun en sonuna eklenecek
    public void FinishHeal()
    {
        isHealing = false;
        currentHealTarget = null;
    }

    

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // İyileştirme menzilini Editor'de Yeşil bir halka ile göster
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRange);
    }
}