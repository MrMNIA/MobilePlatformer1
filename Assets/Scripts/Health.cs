using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 100;
    public float currentHealth {  get; private set; }

    private Animator anim;
    private bool isDead = false;

    [Header("Immunity")]
    [SerializeField] private float immunityTime;
    [SerializeField] private LayerMask ignoreLayer;
    private SpriteRenderer spriteRenderer;
    private bool isImmune = false;

    [SerializeField] private Behaviour[] components;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maximumHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(float damage, Vector3 attackerPosition, float knockbackForce)
    {
        if (isImmune) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);

        if(currentHealth > 0){
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
        Rigidbody2D rib = GetComponent<Rigidbody2D>();

        if (rib != null)
        {
            Vector2 knockbackDirection;
            knockbackDirection.x = Mathf.Sign(transform.position.x - attackerPosition.x);
            knockbackDirection.y = 0.5f;
            knockbackDirection = knockbackDirection.normalized;

            rib.linearVelocity = Vector2.zero;

            rib.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
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

    private void SuddenDeath()
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

        Physics2D.IgnoreLayerCollision(8, 11, true);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(immunityTime - 0.5f);
        spriteRenderer.color = Color.white;

        Physics2D.IgnoreLayerCollision(8, 11, false);
        isImmune = false;

    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

}
