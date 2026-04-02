using UnityEngine;
using UnityEngine.UIElements;
using System;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] public AttackJoystick attackJoystick;
    [SerializeField] public MovementJoystick movementJoystick;
    [SerializeField] private Animator anim;
    private Transform attackCenter;
    public LayerMask enemyLayer;
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private SpriteRenderer attackRange;
    [SerializeField] private PlayerMovement playerMove;
    [SerializeField] private float damage;

    [SerializeField] private AudioClip meleeSound;

    public float attackCooldown = 1f;
    private float attackTimer;
    public bool isAttacking = false;
    private Vector2 lastValidDirection = Vector2.right;

    private void Awake()
    {
        attackCenter = transform.GetChild(0);

        attackCollider = attackCenter.GetChild(0).GetComponent<BoxCollider2D>();
        attackRange = attackCenter.GetChild(0).GetComponent<SpriteRenderer>();

        attackCollider.enabled = false;
        attackRange.enabled = false;

        anim = GetComponent<Animator>();
        attackTimer = 0;

    }
    private void Start() // Awake yerine Start kullanın, daha güvenlidir.
    {
        damage += PlayerPrefs.GetInt("AttackLevel", 0) * 4; // Mağazadan alınan hasar geliştirmesi etkisi

        // 1. Olayı dinlemeye başla (Abone olma)
        if (attackJoystick != null)
        {
            attackJoystick.OnJoystickReleased += meleeAttack;
        }
    }
    private void Update()
    {
        if (attackTimer > 0) { attackTimer -= Time.deltaTime; }

        if (isAttacking) { return; }

        Vector2 targetDirection = new Vector2(attackJoystick.Horizontal, attackJoystick.Vertical);

        if (targetDirection.sqrMagnitude >= 0.2f)
        {
            SetDirection(targetDirection);
            lastValidDirection = targetDirection;
            attackRange.enabled = true;
        }
        else
        {
            attackRange.enabled = false;
        }

    }

    private void SetDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (transform.localScale.x < 0)
        {
            angle = 180 - angle;
        }

        // KRİTİK: localRotation'ı kullanmaya devam edin ve flipOffset'ları kaldırın.
        attackCenter.localRotation = Quaternion.Euler(0f, 0f, angle);
    }



    // 2. Oyundan çıkıldığında dinlemeyi bırak (Bellek sızıntısını önler)
    private void OnDisable()
    {
        if (attackJoystick != null)
        {
            attackJoystick.OnJoystickReleased -= meleeAttack;
        }
    }

    private void meleeAttack()
    {
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            isAttacking = true;
            FlipCharacterToDirection(lastValidDirection.x);
            playerMove.isRotationOverridden = true;


            SetDirection(lastValidDirection);

            anim.SetTrigger("meleeAttack");
            attackJoystick.CooldownCounter(attackCooldown);
            SoundManager.Instance.PlaySound(meleeSound);
        }
    }

    // PlayerMelee.cs (Yeni yardımcı metot)
    private void FlipCharacterToDirection(float xDirection)
    {
        if (xDirection > 0)
            transform.localScale = new Vector3(1, 1, 1); // Sağa çevir
        else if (xDirection < 0)
            transform.localScale = new Vector3(-1, 1, 1); // Sola çevir
    }
    public void DealMeleeDamage()
    {
        attackCollider.enabled = true;
        attackRange.enabled = true;
        Vector2 point = attackCollider.bounds.center;
        Vector2 size = attackCollider.size; // Kutunun boyutları
        float angle = attackCenter.eulerAngles.z; // O anki dönüş açımız

        // 2. O kutunun içindeki düşmanları bul (OverlapBox)
        Collider2D[] hits = Physics2D.OverlapBoxAll(point, size, angle, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            // Health scriptini bul
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Hasar Ver
                enemyHealth.TakeDamage(damage, transform.position, 8f);
            }
        }
    }

    public void EndMelee()
    {
        attackCollider.enabled = false;
        attackRange.enabled = false;
        playerMove.isRotationOverridden = false;
        isAttacking = false;
    }


}
