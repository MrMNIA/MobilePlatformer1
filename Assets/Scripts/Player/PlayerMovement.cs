using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Features")]
    [SerializeField] private float accelerationForce;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float superJumpPower = 35f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private AudioClip jumpSound;
    [HideInInspector] public bool isRotationOverridden = false;
    [SerializeField] public PlayerRespawn playerRespawn;

    [Header("Super Jump Zone")]
    public bool isInSuperJumpZone = false;

    [Header("WallJump")]
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Joysticks & References")]
    [SerializeField] private MovementJoystick move;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayLayer;

    private Coroutine crouchCoroutine;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private Animator anim;
    private float jumpTimer;
    private bool isRunning;
    private bool knockbacked = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        playerEnergy = GetComponent<PlayerEnergy>();
    }

    void Update()
    {
        // 1. Durum Kontrolleri
        isRunning = (onGround() && Mathf.Abs(body.linearVelocity.x) >= 0.2f);
        anim.SetBool("isRunning", isRunning);

        if (onGround())
        {
            anim.SetBool("onGround", true);
            playerRespawn.GetLastValidPosition(transform.position);
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            anim.SetBool("onGround", false);
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 2. Düşüş Animasyonu
        anim.SetBool("isFalling", !onGround() && body.linearVelocity.y < -0.1f);

        // 3. Zıplama Tetikleyici
        if (jumpTimer > 0f) jumpTimer -= Time.deltaTime;

        if ((move.IsJumping || Input.GetKeyDown(KeyCode.Space)) && jumpTimer <= 0f)
            Jump();

        // 4. Dinamik Yerçekimi
        if (!onGround() && onWall() && body.linearVelocity.y < 0f)
        {
            body.gravityScale = 0.3f;
            anim.SetBool("isSliding", true);
        }
        else if (body.linearVelocity.y > 0 && !(Input.GetKey(KeyCode.Space) || move.IsJumping))
        {
            body.gravityScale = 4.0f;
            anim.SetBool("isSliding", false);
        }
        else
        {
            body.gravityScale = 2.0f;
            anim.SetBool("isSliding", false);
        }

        // 5. Platformdan Düşme
        if (move.IsCrouching && onGround())
        {
            if (crouchCoroutine == null)
                crouchCoroutine = StartCoroutine(DisablePlatform());
        }

        // 6. Karakter Yönü
        if (!isRotationOverridden && move.Horizontal != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move.Horizontal), 1, 1);
        }
    }

    private void FixedUpdate()
    {
        if (knockbacked) return;

        float targetSpeed = move.Horizontal * maxSpeed;
        float currentAccel;

        if (move.Horizontal != 0)
        {
            currentAccel = onGround() ? accelerationForce : (accelerationForce / 2f);
        }
        else
        {
            currentAccel = onGround() ? 20f : 5f;
        }

        float newX = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, currentAccel * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(newX, body.linearVelocity.y);
    }

    // --- DIŞARIDAN ÇAĞRILAN GÜÇLENDİRME VE ETKİ METODLARI ---

    public void SpeedBoost(float duration)
    {
        maxSpeed *= 1.5f;
        Invoke(nameof(ResetSpeed), duration);
    }

    private void ResetSpeed()
    {
        maxSpeed /= 1.5f;
    }

    public IEnumerator PlayerKnockbackRoutine(float duration)
    {
        knockbacked = true;
        yield return new WaitForSeconds(duration);
        knockbacked = false;
    }

    // --- ZIPLAMA MANTIĞI ---

    private void Jump()
    {
        if (!playerEnergy.tryUseEnergy(10f)) return;

        float currentEnergyPercentage = playerEnergy.GetEnergyPercentage();

        if (onWall() && !onGround())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0);
            body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX * 50, (wallJumpY + (4 * currentEnergyPercentage)) * 50));
            transform.localScale = new Vector3(transform.localScale.x * -1f, 1, 1);
            ExecuteJumpEffects();
        }
        else if (coyoteTimeCounter > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0);

            // 1. Zıplama Gücü Belirle
            float power = isInSuperJumpZone ? superJumpPower : jumpPower;

            // 2. İleri İtiş Gücü Belirle (Sadece Super Jump bölgesindeyse)
            // localScale.x karakterin baktığı yöndür (1 veya -1)
            float forwardForce = isInSuperJumpZone ? 35f : 0f;

            // 3. Kuvveti Uygula (X eksenine ileri itiş, Y eksenine zıplama gücü)
            body.AddForce(new Vector2(forwardForce * transform.localScale.x * 50, (power + (5 * currentEnergyPercentage)) * 50));

            ExecuteJumpEffects();
            coyoteTimeCounter = 0f;
        }
        jumpTimer = jumpCooldown;
    }

    private void ExecuteJumpEffects()
    {
        playerEnergy.UseEnergy(10f);
        anim.SetTrigger("jump");
        SoundManager.Instance.PlaySound(jumpSound);
    }

    // --- FİZİKSEL KONTROLLER ---

    private IEnumerator DisablePlatform()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.5f, oneWayLayer);
        if (hit.collider != null)
        {
            Collider2D platformCollider = hit.collider;
            Physics2D.IgnoreCollision(boxCollider, platformCollider, true);
            body.linearVelocity = new Vector2(body.linearVelocity.x, -3f);
            yield return new WaitForSeconds(0.35f);
            if (platformCollider != null) Physics2D.IgnoreCollision(boxCollider, platformCollider, false);
        }
        crouchCoroutine = null;
    }

    private bool onGround()
    {
        if (body.linearVelocity.y > 0.1f) return false;
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y + 0.1f);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.6f, 0.02f);
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down, 0.15f, groundLayer | oneWayLayer);
        return hit.collider != null;
    }

    private bool onWall()
    {
        float direction = transform.localScale.x;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size * 0.9f, 0f, new Vector2(direction, 0), 0.2f, groundLayer);
        return hit.collider != null;
    }
}