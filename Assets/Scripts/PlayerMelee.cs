using UnityEngine;
using UnityEngine.UIElements;
using System;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] public AttackJoystick attackJoystick;
    [SerializeField] private Animator anim;
    private Transform attackArea;
    private BoxCollider2D attackCollider;

    public float attackCooldown = 1f;
    private float attackTimer;
    private Vector2 lastValidDirection = Vector2.right;

    private void Awake()
    {
        attackArea = transform.GetChild(0).GetComponent<Transform>();
        attackCollider = attackArea.GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        attackCollider.enabled = false;
    }
    private void Update()
    {
        if(attackTimer > 0) { attackTimer -= Time.deltaTime; }

        Vector2 targetDirection = new Vector2(attackJoystick.Horizontal, attackJoystick.Vertical);

        if (targetDirection.sqrMagnitude >= 0.2f)
        {
            SetDirection(targetDirection);
            lastValidDirection = targetDirection;
        }   
    }

    private void SetDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void Start() // Awake yerine Start kullanýn, daha güvenlidir.
    {
        // 1. Olayý dinlemeye baþla (Abone olma)
        if (attackJoystick != null)
        {
            attackJoystick.OnJoystickReleased += meleeAttack;
        }
    }

    // 2. Oyundan çýkýldýðýnda dinlemeyi býrak (Bellek sýzýntýsýný önler)
    private void OnDisable()
    {
        if (attackJoystick != null)
        {
            attackJoystick.OnJoystickReleased -= meleeAttack;
        }
    }

    private void meleeAttack()
    {
        if(attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            anim.SetTrigger("meleeAttack");
        }
    }

    public void DealMeleeDamage()
    {
        Debug.Log("Melee");
    }
}
