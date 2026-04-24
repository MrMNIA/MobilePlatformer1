using UnityEngine;
using System;
using System.Collections;

public class PlayerMelee : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] public AttackJoystick attackButton;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerMovement playerMove;

    [Header("Attack Area (The Rectangle)")]
    [SerializeField] private Transform attackCenter; // Player altındaki boş obje
    [SerializeField] private BoxCollider2D attackCollider; // attackCenter altındaki dikdörtgen collider
    [SerializeField] private SpriteRenderer attackRange; // attackCenter altındaki görsel dikdörtgen

    [Header("Settings")]
    public LayerMask enemyLayer;
    [SerializeField] private float damage;
    private float effectiveDamage;
    [SerializeField] private AudioClip meleeSound;
    public float attackCooldown = 1f;

    private float attackTimer;
    public bool isAttacking { get; private set; }

    private void Start()
    {
        damage += PlayerPrefs.GetInt("AttackLevel", 0) * 4;
        effectiveDamage = damage;

        if (attackButton != null)
        {
            attackButton.OnAttackPressed += meleeAttack;
        }

        // Başlangıçta her şeyi kapat
        attackCollider.enabled = false;
        attackRange.enabled = false;

        // ÖNEMLİ: Artık manuel açı hesaplaması yapmayacağımız için 
        // attackCenter'ın rotation'ını sıfırlıyoruz.
        attackCenter.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
    }

    private void OnEnable() 
    {
        if (attackButton != null)
            attackButton.OnAttackPressed += meleeAttack;
    }

    private void OnDisable()
    {
        if (attackButton != null)
            attackButton.OnAttackPressed -= meleeAttack;
    }

    private void meleeAttack()
    {
        if (attackTimer <= 0 && !isAttacking)
        {
            isAttacking = true;

            playerMove.isRotationOverridden = true; // Saldırı anında dönüşü kilitle

            anim.SetTrigger("meleeAttack");
            anim.SetBool("isAttacking", true);


            // Failsafe: 1 saniye sonra hala isAttacking true ise zorla kapat
            StartCoroutine(EmergencyReset(1.0f));

            attackTimer = attackCooldown;
            if (attackButton != null)
                attackButton.StartCooldown(attackCooldown);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(meleeSound);
        }
    }

    IEnumerator EmergencyReset(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isAttacking)
        {
            EndMelee();
        }
    }

    // Animasyon Event'i tarafından çağrılır
    public void DealMeleeDamage()
    {
        attackCollider.enabled = true;
        attackRange.enabled = true; // Vuruş anında dikdörtgen görünsün

        // OverlapBox ile vuruş algılama
        // Not: attackCenter zaten Player ile beraber döndüğü için world rotation'ı kullanıyoruz
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackCollider.bounds.center,
            attackCollider.bounds.size,
            attackCenter.eulerAngles.z,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(effectiveDamage, transform.position, 8f);
            }
        }
    }

    // Animasyon sonunda çağrılır
    public void EndMelee()
    {
        attackCollider.enabled = false;
        attackRange.enabled = false;

        if (playerMove != null)
            playerMove.isRotationOverridden = false;

        anim.SetBool("isAttacking", false);
        isAttacking = false;
    }

    public void AttackBoost(float duration)
    {
        effectiveDamage = damage * 1.5f; // Örneğin, hasarı %50 artırabilirsiniz
        Invoke(nameof(ResetAttack), duration);
    }

    private void ResetAttack()
    {
        effectiveDamage = damage;
    }

    private void OnDrawGizmos()
    {
        if (attackCollider == null) return;
        Gizmos.color = Color.red;
        // Collider'ın dünyadaki konumunu ve dönüşünü çizmek için matris kullanıyoruz
        Matrix4x4 rotationMatrix = attackCollider.transform.localToWorldMatrix;
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(attackCollider.offset, attackCollider.size);
    }
}