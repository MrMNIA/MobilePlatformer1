using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.SceneManagement;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maximumHealth = 100;
    public float currentHealth { get; private set; }
    public float damageReduction = 0f; // Hasar azaltma yüzdesi (örneğin, 0.2f = %20 hasar azaltma)
    public static event Action<Health> imDead;


    [Header("Player Respawn")]
    public PlayerRespawn playerRespawn; // PlayerRespawn referansı


    [Header("Immunity")]
    [SerializeField] private float immunityTime;
    private SpriteRenderer spriteRenderer;
    private bool isImmune = false;

    [Range(0f, 100f)] public float knockbackResist = 0f; // %0 = Tam etkilenir, %100 = Hiç kıpırdamaz

    [Header("Enemy")] //d��manlar i�in
    private EnemyAI enemyAI;
    private Coroutine enemyAICoroutine;

    [SerializeField] private Behaviour[] components;
    private Rigidbody2D rb;
    private Animator anim;
    public bool isDead = false;

    public AudioClip hurtSound;
    public AudioClip dieSound;


    private void Awake()
    {
        isDead = false;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Start() {

        if (gameObject.CompareTag("Enemy"))
        {
            // Bölme işleminden gelen küsüratı engellemek için geçici bir hesaplama yapalım
            float levelBonus = SceneManager.GetActiveScene().buildIndex * (maximumHealth / 8f);
            float multiplier = DifficultyManager.Instance.GetStatsMultiplier();

            // Toplam canı hesapla
            float totalHealth = (maximumHealth + levelBonus) * multiplier;

            // Sonucu en yakın tam sayıya yuvarla (Örn: 12.4 -> 12, 12.6 -> 13)
            maximumHealth = Mathf.Round(totalHealth);

            // VEYA her zaman yukarı yuvarla (Örn: 12.1 bile olsa 13 yapar, düşmanlar zayıf kalmasın dersen)
            // maximumHealth = Mathf.Ceil(totalHealth);

            currentHealth = maximumHealth;
        }

        if (gameObject.CompareTag("Player"))
        {
            maximumHealth += PlayerPrefs.GetInt("HealthLevel", 0) * 10; // Mağazadan alınan can geliştirmesi etkisi
            currentHealth = maximumHealth;
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition, float knockbackForce)
    {
        if (isImmune) return;

        isImmune = true; // Hasar aldıktan sonra geçici olarak dokunulmaz yap
        if (immunityTime == 0)
        {
            isImmune = false; // Eğer immunityTime 0 ise, dokunulmazlığı hemen kaldır
        }

        damage *= (1f - damageReduction); // Hasarı azalt
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);

        if (currentHealth > 0)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Player_MeleeAttack"))
            {
                anim.SetTrigger("hurt");
            }
            HurtAffect();
            SoundManager.Instance.PlaySound(hurtSound);
            Knockback(attackerPosition, knockbackForce);
            if (immunityTime > 0)
            {
                StartCoroutine(Immunity(immunityTime));
            }
        }
        else
        {
            DieSequence();
        }
    }

    private void Knockback(Vector3 attackerPosition, float knockbackForce)
    {
        if (rb == null || knockbackResist >= 100f) return;

        float effectiveForce = knockbackForce * (1f - (knockbackResist / 100f));
        if (effectiveForce <= 0.1f) return;

        if (enemyAI != null)
        {
            if (enemyAICoroutine != null) StopCoroutine(enemyAICoroutine);
            enemyAICoroutine = StartCoroutine(DisableEnemyAI(0.25f));
        }

        if (CompareTag("Player"))
        {
            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null) StartCoroutine(pm.PlayerKnockbackRoutine(0.25f));
        }

        // Yön hesaplama
        float directionX = (transform.position.x - attackerPosition.x) > 0 ? 1f : -1f;
        Vector2 finalDirection = new Vector2(directionX, 0.5f).normalized;

        // --- DEĞİŞİKLİK BURADA BAŞLIYOR ---

        // 1. Mevcut hızı sıfırlamak (Opsiyonel ama önerilir): 
        // Eğer bunu yapmazsanız, karakter zaten zıt yöne çok hızlı gidiyorsa knockback onu durdurmaya yetmeyebilir.

        // 2. AddForce Kullanımı:
        // ForceMode2D.Impulse kullanıyoruz çünkü knockback anlık bir patlama etkisidir.
        rb.AddForce(finalDirection * effectiveForce, ForceMode2D.Impulse);
    }

    private IEnumerator DisableEnemyAI(float duration)
    {
        if (enemyAI != null) { enemyAI.enabled = false; }
        yield return new WaitForSeconds(duration);
        if (enemyAI != null) enemyAI.enabled = true;
        enemyAICoroutine = null;
    }

    private void DieSequence()
    {
        if (isDead) return;
        isDead = true;

        anim.SetTrigger("die");
        SoundManager.Instance.PlaySound(dieSound);

        // --- COIN SİSTEMİ ENTEGRASYONU ---
        // Eğer ölen bir düşmansa, cüzdana para ekle
        if (gameObject.CompareTag("Enemy") && enemyAI != null)
        {
            // EnemyAI scriptindeki baseCoinReward değerini kullanıyoruz

            MoneyManager.Instance.AddCoinsToCurrentWallet(enemyAI.baseCoinReward);
            imDead?.Invoke(this);
        }

        // Bileşenleri kapat
        foreach (var item in components)
        {
            item.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Artık fizik dünyasıyla etkileşime girmesin
        }
    }

    public void PlayerDown()
    {
        if(playerRespawn != null && !playerRespawn.IsRespawned())
        {
            UIManager.instance.ShowAlmostGameOver();
        }
        else
        {
            UIManager.instance.ShowGameOver();
        }
    }
    public void SuddenDeath()
    {
        currentHealth = 0f;
        DieSequence();
    }
    public void AddHealth(float heal)
    {
        currentHealth += heal;
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);
    }

    private IEnumerator HurtAffect()
    {
        Color defaultColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = defaultColor;
    }
    private IEnumerator Immunity(float immunityTime)
    {
        isImmune = true;
        Color defaultColor = spriteRenderer.color;
        Color flashColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.35f);
        float blinkDuration = 0.25f;
        float colorTimer = 0f;

        while (colorTimer < immunityTime - 0.1f)
        {
            // Yar� saydam/Soluk renge ge�
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(blinkDuration);

            // Orijinal renge geri d�n
            spriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(blinkDuration);

            colorTimer += (blinkDuration * 2);
        }

        spriteRenderer.color = defaultColor;

        isImmune = false;

    }
    
    public void StartRespawn()
    {
        gameObject.SetActive(true);
        anim.SetTrigger("Respawn");
        currentHealth = maximumHealth;
        isDead = false;
    }

    public void EndRespawn()
    {
        foreach (var item in components)
        {
            item.enabled = true;
        }
        if (rb != null)
        {
            rb.simulated = true; // Fizik etkileşimini tekrar aç
        }
        StartCoroutine(Immunity(2.5f)); // Kısa süreli dokunulmazlık ver (örneğin, 1 saniye)
    }

    public void ShieldBoost(float duration)
    {
        damageReduction = 0.5f; // Örneğin, %50 hasar azaltma
        anim.SetBool("haveShield",true);
        Invoke(nameof(ResetShield), duration);
    }

    private void ResetShield()
    {
        damageReduction = 0f;
        anim.SetBool("haveShield", false);

    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public float GetCurrentHealthPercent()
    {
        return currentHealth / maximumHealth;
    }

}
