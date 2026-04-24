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
    [SerializeField] public PlayerRespawn playerRespawn;

    [Header("WallJump")]
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f; // Havada ne kadar süre zıplama hakkı tanınsın?
    private float coyoteTimeCounter;

    [Header("Joysticks & References")]
    [SerializeField] private MovementJoystick move;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayLayer;

    private Coroutine crouchCoroutine;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private Animator anim;
    private float currentAcceleration;
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
        isRunning = (onGround() && Mathf.Abs(body.linearVelocity.x) >= 0.2f);
        anim.SetBool("isRunning", isRunning);

        if (onGround())
        {
            anim.SetBool("onGround", true);
            playerRespawn.GetLastValidPosition(transform.position); // Geçerli pozisyonu kaydet
            coyoteTimeCounter = coyoteTime; // Yerdeyken sayacı tazele
        }
        else
        {
            anim.SetBool("onGround", false);
            coyoteTimeCounter -= Time.deltaTime; // Havadaysak süreyi azalt
        }

        if (!onGround() && body.linearVelocity.y < 0.2f)
            anim.SetBool("isFalling", true);
        else
            anim.SetBool("isFalling", false);

        if (jumpTimer > 0f)
            jumpTimer -= Time.deltaTime;
        if ((move.IsJumping || Input.GetKeyDown(KeyCode.Space)) && jumpTimer <= 0f) //joystick ya da space basıldığında
            Jump();

        if (move.IsCrouching && onGround())
        {
            if (crouchCoroutine == null)      
                crouchCoroutine = StartCoroutine(DisablePlatform());
        }

        // Duvar sürtünmesi (Wall Slide)
        if (!onGround() && onWall() && body.linearVelocity.y < 0f)
        {
            body.gravityScale = 0.3f;
            anim.SetBool("isSliding", true);
        }
        else
        {
            body.gravityScale = 2.0f;
            anim.SetBool("isSliding", false);
        }

        // Karakter Yönü Çevirme
        if (!isRotationOverridden && move.Horizontal != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move.Horizontal), 1, 1);
        }
    }

    private IEnumerator DisablePlatform()
    {
        // 1. Karakterin tam altındaki platformu bulalım
        // (BoxCast kullanarak karakterin bastığı objeyi alıyoruz)
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.5f, oneWayLayer);

        if (hit.collider != null)
        {
            Collider2D platformCollider = hit.collider;

            // 2. Karakterin collider'ı ile platformun collider'ı arasındaki çarpışmayı devre dışı bırak
            Physics2D.IgnoreCollision(boxCollider, platformCollider, true);
            Debug.Log("Collision disabled for: " + platformCollider.name);

            // 3. Karakteri aşağı doğru it (Yerçekimini tetiklemek için)
            body.linearVelocity = new Vector2(body.linearVelocity.x, -3f);

            // 4. Kısa bir süre bekle (Platformun içine girmesi için)
            yield return new WaitForSeconds(0.2f);

            // 5. Karakter platformdan tamamen çıkana kadar bekle
            // Burada hala IsInsidePlatform() kullanabilirsin
            while (IsInsidePlatform())
            {
                yield return null;
            }

            // 6. Çarpışmayı tekrar aç
            Physics2D.IgnoreCollision(boxCollider, platformCollider, false);
            Debug.Log("Collision re-enabled.");
        }

        crouchCoroutine = null;
    }
    private bool IsInsidePlatform()
    {
        return Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0, oneWayLayer);
    }
    private void FixedUpdate()
    {
        if(knockbacked) return;
        // 1. Hedef Hızı Belirle: Tuşa basılıyorsa maxSpeed, basılmıyorsa 0.
        float targetSpeed = move.Horizontal * maxSpeed;

        // 2. İvme/Yavaşlama Hızını Belirle
        float currentAccel;
        
        if (move.Horizontal != 0)
        {
            // Hareket halindeyken (Yerdeyken tam güç, havadayken yarım güç)
            currentAccel = onGround() ? accelerationForce : (accelerationForce / 2f);
        }
        else
        {
            // Dururken (Sürtünme etkisi gibi düşün)
            // Eğer yerdeysek hızlı dur (20f), havadaysak yavaş dur (2f)
            currentAccel = onGround() ? 20f : 5f; 
        }

        // 3. Mevcut hızı, hedef hıza (targetSpeed) 'currentAccel' hızıyla yaklaştır
        // MoveTowards asla hedefi aşmaz, bu yüzden "tampon kuvvet" gibi ters itme yapmaz.
        float newX = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, currentAccel * Time.fixedDeltaTime);

        // 4. Yeni hızı uygula (Y eksenindeki hızı/yerçekimini koruyoruz)
        body.linearVelocity = new Vector2(newX, body.linearVelocity.y);
    }

    public void SpeedBoost(float duration)
    {
        maxSpeed *= 1.5f; // Örneğin, hızı %50 artırabilirsiniz
        Invoke(nameof(ResetSpeed), duration);
    }

    private void ResetSpeed()
    {
        maxSpeed /= 1.5f;
    }

    public IEnumerator PlayerKnockbackRoutine(float duration)
    {
        knockbacked = true; // Hareket kodlarını kilitler
        yield return new WaitForSeconds(duration);
        knockbacked = false; // Hareket kodlarını tekrar açar
    }
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
            body.AddForce(new Vector2(0, (jumpPower + (5 * currentEnergyPercentage)) * 50));
            ExecuteJumpEffects();
            
            // Zıpladıktan sonra tekrar havada zıplayamasın diye sayacı sıfırla
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

    private bool onGround()
    {
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);

        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down, 0.05f, groundLayer | oneWayLayer);
        return hit.collider != null;
    }

    private bool onWall()
    {
        // Karakterin baktığı yöne göre küçük bir ofset belirliyoruz
        float direction = transform.localScale.x;
        Vector2 rayDirection = new Vector2(direction, 0);

        // BoxCast'i karakterin biraz dışından başlatmak veya mesafeyi netleştirmek gerekir
        // 0.1f veya 0.2f mesafe (distance) duvarı algılamak için idealdir.
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size * 0.9f, // Kutuyu hafif küçültüyoruz ki tavan/tabana çarpmasın
            0f,
            rayDirection,
            0.2f,
            groundLayer
        );

        return hit.collider != null;
    }


    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        // BoxCast'in gittiği yeri sahnede kutu olarak çizer
        Gizmos.DrawWireCube(boxCollider.bounds.center + new Vector3(transform.localScale.x * 0.2f, 0, 0), boxCollider.bounds.size * 0.9f);
    }

}