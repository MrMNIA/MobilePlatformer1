using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 100;
    public float currentHealth { get; private set; }

    [Header("Immunity")]
    [SerializeField] private float immunityTime;
    private SpriteRenderer spriteRenderer;
    private bool isImmune = false;

    [Header("Enemy")] //d��manlar i�in
    private EnemyAI enemyAI;
    private Coroutine enemyAICoroutine;

    [SerializeField] private Behaviour[] components;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    public AudioClip hurtSound;
    public AudioClip dieSound;


    private void Awake()
    {
        isDead = false;
        currentHealth = maximumHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        // --- ZORLUK SİSTEMİ ENTEGRASYONU ---
        // Eğer bu obje bir düşmansa, zorluğa göre canını artırıyoruz.
        if (gameObject.CompareTag("Enemy"))
        {
            float multiplier = DifficultyManager.Instance.GetStatsMultiplier();
            maximumHealth *= multiplier;
            currentHealth = maximumHealth;
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition, float knockbackForce)
    {
        if (isImmune) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);

        if (currentHealth > 0)
        {
            anim.SetTrigger("hurt");
            SoundManager.Instance.PlaySound(hurtSound);
            Knockback(attackerPosition, knockbackForce);
            StartCoroutine(Immunity(immunityTime));
        }
        else
        {
            DieSequence();
        }
    }

    private void Knockback(Vector3 attackerPosition, float knockbackForce)
    {

        if (rb != null)
        {
            if (enemyAI != null)
            {
                if (enemyAICoroutine != null)
                {
                    StopCoroutine(enemyAICoroutine);
                }
                enemyAICoroutine = StartCoroutine(DisableEnemyAI(0.25f));
            }

            Vector2 knockbackDirection;
            knockbackDirection.x = Mathf.Sign(transform.position.x - attackerPosition.x);
            knockbackDirection.y = 0.5f;
            knockbackDirection = knockbackDirection.normalized;

            rb.linearVelocity = Vector2.zero;

            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
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
            LevelManager.Instance.AddCoins(enemyAI.baseCoinReward);
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

    public void GameOver()
    {
        if (gameObject.CompareTag("Player"))
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

    private IEnumerator Immunity(float immunityTime)
    {
        if (immunityTime <= 0) yield break;
        isImmune = true;

        Color defaultColor = spriteRenderer.color;
        Color flashColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.35f);
        float blinkDuration = 0.25f;
        float colorTimer = 0f;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

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

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

}
