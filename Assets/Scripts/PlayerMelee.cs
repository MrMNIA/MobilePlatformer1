using UnityEngine;
using UnityEngine.UIElements;
using System;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] public AttackJoystick attackJoystick;
    [SerializeField] private Animator anim;
    private Transform attackCenter;
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private SpriteRenderer attackRange;
    [SerializeField] private PlayerMovement playerMove;

    public float attackCooldown = 1f;
    private float attackTimer;
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
    private void Update()
    {
        if (attackTimer > 0) { attackTimer -= Time.deltaTime; }

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
            attackCenter.localScale = new Vector3(-1,1,1);
            angle = -(angle);
        }
        else
        {
            attackCenter.localScale = new Vector3(1,1,1);
        }

        // Yalnızca görsel ofseti uygulayın (örn. nişan alma görseliniz yukarı bakıyorsa -90f)
        const float visualDefaultOffset = 0f;

        // KRİTİK: localRotation'ı kullanmaya devam edin ve flipOffset'ları kaldırın.
        attackCenter.localRotation = Quaternion.Euler(0f, 0f, angle + visualDefaultOffset);
    }

    private void Start() // Awake yerine Start kullanın, daha güvenlidir.
    {
        // 1. Olayı dinlemeye başla (Abone olma)
        if (attackJoystick != null)
        {
            attackJoystick.OnJoystickReleased += meleeAttack;
        }
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
            FlipCharacterToDirection(lastValidDirection.x);
            playerMove.isRotationOverridden = true;
            SetDirection(lastValidDirection);
            anim.SetTrigger("meleeAttack");
            attackJoystick.CooldownCounter(attackCooldown);
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
        Debug.Log("Melee");
    }

    public void EndMelee()
    {
        attackCollider.enabled = false;
        attackRange.enabled = false;
        playerMove.isRotationOverridden = false;
    }


}
