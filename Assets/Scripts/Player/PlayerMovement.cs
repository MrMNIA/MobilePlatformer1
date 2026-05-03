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

        // 4. DİNAMİK YERÇEKİMİ YÖNETİMİ (Buraya dikkat)
        // Wall Slide durumu en öncelikli
        if (!onGround() && onWall() && body.linearVelocity.y < 0f)
        {
            body.gravityScale = 0.3f;
            anim.SetBool("isSliding", true);
        }
        // Değişken Zıplama: Karakter yükseliyor ama oyuncu tuşa basmıyorsa yerçekimini artır
        else if (body.linearVelocity.y > 0 && !(Input.GetKey(KeyCode.Space) || move.IsJumping))
        {
            body.gravityScale = 4.0f; // Tuşu bıraktığında "ağırlaşır" ama ivmesi ölmez
            anim.SetBool("isSliding", false);
        }
        // Normal durum (Yerdeyken veya zıplarken tuş basılıyken)
        else
        {
            body.gravityScale = 2.0f;
            anim.SetBool("isSliding", false);
        }

        if (move.IsCrouching && onGround())
        {
            if (crouchCoroutine == null)
                crouchCoroutine = StartCoroutine(DisablePlatform());
        }

        // 5. Karakter Yönü
        if (!isRotationOverridden && move.Horizontal != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move.Horizontal), 1, 1);
        }
    }

    private IEnumerator DisablePlatform()
    {
        // Karakterin tam altındaki platformu bul
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.5f, oneWayLayer);

        if (hit.collider != null)
        {
            Collider2D platformCollider = hit.collider;

            // Çarpışmayı devre dışı bırak
            Physics2D.IgnoreCollision(boxCollider, platformCollider, true);

            // Karakteri aşağı doğru ufak bir hızla it ki düşme başlasın
            body.linearVelocity = new Vector2(body.linearVelocity.x, -3f);

            // Sabit bir süre bekle (0.3sn - 0.4sn genellikle platformdan geçmek için yeterlidir)
            yield return new WaitForSeconds(0.35f);

            // Çarpışmayı tekrar aç
            if (platformCollider != null)
            {
                Physics2D.IgnoreCollision(boxCollider, platformCollider, false);
            }
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
        // EĞER KARAKTER YUKARI DOĞRU HIZLI ÇIKIYORSA, ASLA YERDE SAYILMA
        // Bu, platformun içinden geçerken 'onGround' olup takılmanı önler.
        if (body.linearVelocity.y > 0.1f) return false;

        float extraHeight = 0.1f; // Biraz daha dar bir tolerans
        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y + extraHeight);
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.6f, 0.02f);

        // Işın mesafesini çok kısa tut (0.12f gibi), sadece ayak tabanının hemen altını kontrol etsin
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down, 0.15f, groundLayer | oneWayLayer);

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
        if (boxCollider != null)
        {
            // 1. BoxCast'in başladığı merkezi ve boyutu belirle (onGround fonksiyonunla aynı)
            Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
            Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);
            float distance = 0.05f;
            Vector2 direction = Vector2.down;

            // 2. Çizim rengini ayarla
            Gizmos.color = Color.red;

            // 3. Başlangıç kutusunu çiz (Karakterin ayak tabanındaki hali)
            Gizmos.DrawWireCube(boxCenter, boxSize);

            // 4. Hedef kutuyu çiz (Fırlatıldığı son nokta)
            Vector2 endCenter = boxCenter + direction * distance;
            Gizmos.color = Color.green; // Ulaştığı yer farklı renk olsun
            Gizmos.DrawWireCube(endCenter, boxSize);

            // 5. Aradaki mesafeyi göstermek için çizgiler çek (Opsiyonel)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(boxCenter, endCenter);
        }
    }
}