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

    [Header("Enemy")] //düþmanlar için
    private EnemyAI enemyAI;
    private Coroutine enemyAICoroutine;

    [SerializeField] private Behaviour[] components;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maximumHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
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
        anim.SetTrigger("die");

        foreach (var item in components)
        {
            item.enabled = false;
        }

        rb.simulated = false;
            
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
            // Yarý saydam/Soluk renge geç
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(blinkDuration);

            // Orijinal renge geri dön
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
