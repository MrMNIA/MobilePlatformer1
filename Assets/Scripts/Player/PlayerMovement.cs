using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Features")]
    [SerializeField] private float accelerationForce;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private AudioClip jumpSound;
    [HideInInspector] public bool isRotationOverridden = false;

    [Header("WallJump")]
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;

    [Header("Joysticks & References")]
    [SerializeField] private MovementJoystick movementJoystick;
    [SerializeField] private AttackJoystick attackJoystick;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayLayer;

    private Coroutine disableCollisionCoroutine;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private Animator anim;

    private float horizontalInput;
    private float verticalInput;
    private float currentAcceleration;
    private float jumpTimer;
    private bool isRunning;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        playerEnergy = GetComponent<PlayerEnergy>();
    }

    void Update()
    {
        horizontalInput = movementJoystick.Horizontal;
        verticalInput = movementJoystick.Vertical;

        anim.SetBool("onGround", onGround());
        isRunning = (onGround() && Mathf.Abs(body.linearVelocity.x) >= 0.2f);
        anim.SetBool("isRunning", isRunning);

        if (jumpTimer > 0f)
            jumpTimer -= Time.deltaTime;

        if (verticalInput >= 0.6f && jumpTimer <= 0f)
            Jump();

        if (verticalInput <= -0.6f && onGround())
        {
            // Eğer ayağımızın altındaki obje "OneWay" katmanındaysa düşmeyi başlat
            if (IsOnOneWayPlatform())
            {
                if (disableCollisionCoroutine == null)
                    disableCollisionCoroutine = StartCoroutine(DisableCollision());
            }
        }

        // Duvar sürtünmesi (Wall Slide)
        if (!onGround() && onWall() && body.linearVelocity.y < 0f)
            body.gravityScale = 0.3f;
        else
            body.gravityScale = 2.0f;

        // Karakter Yönü Çevirme
        if (!isRotationOverridden && Mathf.Abs(horizontalInput) >= 0.2f)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
    }

    private void FixedUpdate()
    {
        // Hareket ve Hız Kontrolü
        if (Mathf.Abs(horizontalInput) >= 0.25f)
        {
            currentAcceleration = onGround() ? accelerationForce : (accelerationForce / 2);
            body.AddForce(new Vector2(horizontalInput * currentAcceleration, 0));
        }
        else if (Mathf.Abs(body.linearVelocity.x) > 0.1f)
        {
            body.AddForce(new Vector2(-body.linearVelocity.x * 20f, 0));
        }
        else
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
        }

        // Azami Hız Sınırı
        if (Mathf.Abs(body.linearVelocity.x) > maxSpeed)
        {
            body.linearVelocity = new Vector2(Mathf.Sign(body.linearVelocity.x) * maxSpeed, body.linearVelocity.y);
        }
    }

    // Yeni: Tek yönlü platformda olup olmadığımızı kontrol eden metod
    // YENİ: Sadece OneWay platformda mı duruyoruz?
    private bool IsOnOneWayPlatform()
    {
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down, 0.1f, oneWayLayer);
        return hit.collider != null;
    }

    // YENİ: Çarpışmayı geçici olarak kapatan Coroutine
    private IEnumerator DisableCollision()
    {
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);

        // Tilemap üzerindeki collider'ı buluyoruz
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down, 0.1f, oneWayLayer);

        if (hit.collider != null)
        {
            Collider2D platformCollider = hit.collider;

            // TilemapCollider2D veya CompositeCollider2D ile çarpışmayı keser
            Physics2D.IgnoreCollision(boxCollider, platformCollider, true);

            // 0.4 saniye platformun içinden geçmek için yeterlidir
            yield return new WaitForSeconds(0.4f);

            if (platformCollider != null)
            {
                Physics2D.IgnoreCollision(boxCollider, platformCollider, false);
            }
        }

        disableCollisionCoroutine = null;
    }

    private void Jump()
    {
        if (!playerEnergy.tryUseEnergy(10f)) return;

        if (onWall() && !onGround())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0);
            body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX * 50, wallJumpY * 50));
            ExecuteJumpEffects();
        }
        else if (onGround())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0);
            body.AddForce(new Vector2(0, jumpPower * 50));
            ExecuteJumpEffects();
        }
        jumpTimer = jumpCooldown;
    }

    private void ExecuteJumpEffects()
    {
        playerEnergy.UseEnergy(10f);
        anim.SetTrigger("jump");
        SoundManager.Instance.PlaySound(jumpSound);
    }

    private bool onGround()
    {
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);

        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down, 0.05f, groundLayer | oneWayLayer);
        return hit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.2f, groundLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) return;

        Gizmos.color = onGround() ? Color.green : Color.red;
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);
        Vector3 drawCenter = new Vector3(boxCenter.x, boxCenter.y - 0.05f, 0);

        Gizmos.DrawWireCube(drawCenter, new Vector3(boxSize.x, boxSize.y, 0));
    }
}