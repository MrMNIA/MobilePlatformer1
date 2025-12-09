using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 100;
    public float currentHealth { get; private set; }

    private Animator anim;
    private bool isDead = false;

    [Header("Immunity")]
    [SerializeField] private float immunityTime;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask enemyLayerMask;
    private SpriteRenderer spriteRenderer;
    private bool isImmune = false;
    private Color defaultColor;

    [Header("Enemy")]
    private EnemyAI enemyAI;
    private Coroutine enemyAICoroutine;

    [SerializeField] private Behaviour[] components;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maximumHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();

        defaultColor = spriteRenderer.color;
    }

    public void TakeDamage(float damage, Vector3 attackerPosition, float knockbackForce)
    {
        if (isImmune) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);

        if (currentHealth > 0)
        {
            anim.SetTrigger("hurt");
            Knockback(attackerPosition, knockbackForce);
            StartCoroutine(Immunity());
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
        // Bu blok, TakeDamage metodundaki 'else' bloðundan taþýnmýþtýr.

        // Animasyonu tetikle
        anim.SetTrigger("die");

        // Karakterin diðer bileþenlerini devre dýþý býrak
        foreach (var item in components)
        {
            item.enabled = false;
        }

        // Fizik simülasyonunu durdur
        rb.simulated = false;

        // Durum bayraðýný ayarla
        isDead = true;
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

    private IEnumerator Immunity()
    {
        isImmune = true;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.5f);

        spriteRenderer.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.5f);

        yield return new WaitForSeconds(immunityTime - 0.5f);

        spriteRenderer.color = defaultColor;

        isImmune = false;

    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

}
