using UnityEngine;
using System;
using System.Collections;

public class GorillaBossAI : EnemyAI
{
    public static event Action OnGorillaEnraged;

    [Header("Referanslar")]
    public GorillaBars uiManager;
    private Health bossHealth;
    private SpriteRenderer spriteRenderer;

    [Header("Saldırı Ayarları")]
    public float meleeDamage = 25f;
    public float meleeKnockback = 15f;
    public float meleeCooldown = 1.2f;
    public float meleeDistance = 4.5f;
    public float specialAttackCooldown = 5f;
    public LayerMask playerLayer;

    [Header("Melee İnce Ayar (Hitbox)")]
    public Vector2 meleeOffset = new Vector2(2f, 0f); // X ve Y ofsetleri
    public Vector2 meleeSize = new Vector2(4.5f, 4.5f); // Vuruş alanının boyutları

    [Header("Slam Ayarları")]
    public float slamRecoveryTime = 1.0f;
    public GameObject slamWaveManagerPrefab;
    public float jumpSlamHeight = 18f;
    public float jumpSlamForwardForce = 7f;

    [Header("Dinamik İhtimal Sistemi")]
    [Range(0, 100)] public int currentRockWeight = 100;
    [Range(0, 100)] public int currentSlamWeight = 0;
    [Range(0, 100)] public int currentPowerWeight = 0;

    [Header("Özel Yetenekler")]
    public GameObject rockPrefab;
    public Transform throwPoint;
    public float throwForce = 12f;

    [Header("Hareket & Navigasyon")]
    public float verticalJumpForce = 15f;
    public float verticalNavCooldown = 2.0f;
    private float verticalNavTimer;

    [Header("Durumlar")]
    public bool isBossRaged = false;
    public bool isDying = false;
    private bool isSlamming = false;
    private bool isDropping = false;
    private float meleeTimer;
    private float specialAttackTimer;

    protected override void Awake()
    {
        base.Awake();
        bossHealth = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        attackRangeX = meleeDistance;
        stopDistanceX = meleeDistance * 0.85f;
        specialAttackTimer = 3f;
    }

    protected override void Update()
    {
        if (state == State.Stasis || player == null || !player.gameObject.activeSelf) return;

        if (isDying || (bossHealth != null && bossHealth.isDead))
        {
            state = State.Stasis;
            rib.linearVelocity = new Vector2(0, rib.linearVelocity.y);
            anim.SetBool("isRunning", false);
            return;
        }

        CheckRageThreshold();
        UpdateTimersAndWeights();

        if (!isAttacking && !isSlamming && !isDropping)
        {
            HandleCombatLogic();

            float xDiff = Mathf.Abs(transform.position.x - player.position.x);
            if (!isAttacking && xDiff <= stopDistanceX && IsGrounded() && verticalNavTimer <= 0)
            {
                HandleVerticalNavigation();
            }
        }

        UpdatePhysicsAndAnims();
        base.Update();
    }

    // --- ZIPLAMA VE NAVİGASYON ---

    public void ExecuteExternalJump(float force, float forwardBoost)
    {
        if (isDying || state == State.Stasis) return;

        float dir = movingRight ? 1f : -1f;
        rib.linearVelocity = new Vector2(rib.linearVelocity.x + (dir * forwardBoost), force);
        anim.SetTrigger("jump");
    }

    private void HandleVerticalNavigation()
    {
        if (verticalNavTimer > 0) return;

        float yDiff = player.position.y - transform.position.y;

        if (yDiff > 2f)
        {
            rib.linearVelocity = new Vector2(rib.linearVelocity.x, verticalJumpForce);
            anim.SetTrigger("jump");
            verticalNavTimer = verticalNavCooldown;
        }
        else if (yDiff < -2f && IsOnOneWayPlatform())
        {
            StartCoroutine(DisablePlatformRoutine());
            verticalNavTimer = verticalNavCooldown;
        }
    }

    // --- SALDIRI MANTIĞI ---

    public override void Attack() { }

    protected override bool ReadyToAttack()
    {
        bool isStableOnGround = IsGrounded() && Mathf.Abs(rib.linearVelocity.y) <= 0.1f;
        return !isAttacking && !isSlamming && !isDropping && meleeTimer <= 0 && isStableOnGround;
    }

    private void HandleCombatLogic()
    {
        float xDiff = Mathf.Abs(transform.position.x - player.position.x);
        float yDiffAbs = Mathf.Abs(transform.position.y - player.position.y);

        if (isBossRaged)
        {
            if (xDiff <= meleeDistance && yDiffAbs <= 2.5f && ReadyToAttack())
                ExecuteMeleeAttack();
        }
        else
        {
            if (specialAttackTimer <= 0 && IsGrounded())
                ExecuteSpecialAttackByWeight();
            else if (xDiff <= meleeDistance && yDiffAbs <= 2.5f && ReadyToAttack())
                ExecuteMeleeAttack();
        }
    }

    private void ExecuteMeleeAttack()
    {
        isAttacking = true;
        meleeTimer = meleeCooldown;
        ApplyMovement(0);
        FacePlayer();
        anim.SetTrigger("meleeAttack");
    }

    private void ExecuteSpecialAttackByWeight()
    {
        isAttacking = true;
        specialAttackTimer = specialAttackCooldown;
        ApplyMovement(0);
        FacePlayer();

        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < currentRockWeight) anim.SetTrigger("rangedAttack");
        else if (roll < (currentRockWeight + currentSlamWeight)) StartJumpSlam();
        else
        {
            anim.SetTrigger("powerShow");
            if (bossHealth != null) bossHealth.AddHealth(100f);
        }
    }

    // --- ANIMASYON EVENTLERI VE VURUŞ ALANI ---

    public void Event_DealMeleeDamage()
    {
        float dir = movingRight ? 1f : -1f;

        // Yeni ofset ve parametrelerle merkez noktasını hesapla
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(meleeOffset.x * dir, meleeOffset.y);

        // Alan kontrolü (BoxCast yerine OverlapBox daha temizdir ama BoxCast de mesafe 0 ile aynı işi görür)
        Collider2D hit = Physics2D.OverlapBox(boxCenter, meleeSize, 0f, playerLayer);

        if (hit != null)
        {
            Health pHealth = hit.GetComponent<Health>();
            if (pHealth != null)
            {
                float d = isBossRaged ? meleeDamage * 2f : meleeDamage;
                float k = isBossRaged ? meleeKnockback * 2f : meleeKnockback;
                pHealth.TakeDamage(d, transform.position, k);
            }
        }
    }

    // Editörde vuruş alanını görmek için Gizmos
    private void OnDrawGizmosSelected()
    {
        float dir = movingRight ? 1f : -1f;
        // Editörde henüz oyun başlamamışsa sağa bakıyormuş gibi çizelim
        if (!Application.isPlaying) dir = 1f;

        Vector2 drawPos = (Vector2)transform.position + new Vector2(meleeOffset.x * dir, meleeOffset.y);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(drawPos, meleeSize);
    }

    // --- FİZİK, GROUNDED VE DİĞER (DEĞİŞMEDİ) ---

    private void UpdatePhysicsAndAnims()
    {
        bool grounded = IsGrounded();
        if (!grounded) anim.SetBool("isRunning", false);
        anim.SetBool("isFalling", !grounded && (rib.linearVelocity.y < -0.1f || (!isSlamming && rib.linearVelocity.y < 0.1f)));
        anim.SetBool("isSlamming", isSlamming && !grounded);

        if (isSlamming && !grounded)
        {
            float xDir = (player.position.x > transform.position.x) ? 1f : -1f;
            rib.linearVelocity = new Vector2(xDir * jumpSlamForwardForce, rib.linearVelocity.y);
        }

        if (isSlamming && grounded && rib.linearVelocity.y <= 0.1f && !IsInvoking(nameof(ResetEylemler)))
        {
            ExecuteSlamImpact();
        }

        int platformLayer = (int)Mathf.Log(oneWayPlatformLayer.value, 2);
        if (rib.linearVelocity.y > 0.1f || (isSlamming && !grounded))
            Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayer, true);
        else if (!isDropping)
            Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayer, false);

        if (!grounded && Mathf.Abs(rib.linearVelocity.y) < 0.1f && !isAttacking && !isSlamming)
        {
            float xDir = (player.position.x > transform.position.x) ? 1f : -1f;
            rib.linearVelocity = new Vector2(xDir * 2f, rib.linearVelocity.y);
        }
    }

    protected override bool IsGrounded()
    {
        if (rib.linearVelocity.y > 0.1f) return false;
        if (isDropping || IsInsidePlatform()) return false;
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.9f, 0.1f);
        float checkDist = boxCollider.bounds.extents.y + 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0f, Vector2.down, checkDist, combinedLayer);
        return hit.collider != null;
    }

    private IEnumerator DisablePlatformRoutine()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.7f, oneWayPlatformLayer);
        if (hit.collider != null)
        {
            isDropping = true;
            Collider2D platformCollider = hit.collider;
            Physics2D.IgnoreCollision(boxCollider, platformCollider, true);
            rib.linearVelocity = new Vector2(rib.linearVelocity.x, -5f);
            yield return new WaitForSeconds(0.4f);
            if (platformCollider != null)
                Physics2D.IgnoreCollision(boxCollider, platformCollider, false);
        }
        isDropping = false;
    }

    private bool IsInsidePlatform() => Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size * 0.9f, 0, oneWayPlatformLayer);
    private bool IsOnOneWayPlatform() => Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 1.2f, oneWayPlatformLayer).collider != null;

    private void CheckRageThreshold()
    {
        if (isBossRaged || bossHealth == null) return;
        if (bossHealth.GetCurrentHealthPercent() <= 0.2f)
        {
            isBossRaged = true;
            state = State.Stasis;
            rib.linearVelocity = Vector2.zero;
            isAttacking = false;
            isSlamming = false;
            anim.SetBool("isRunning", false);
            anim.SetTrigger("intro");
            bossHealth.AddHealth(bossHealth.maximumHealth);
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.4f, 0.4f);
            if (uiManager != null) uiManager.ActivateEnrageUI();
            bossHealth.damageReduction = 1f;
            chaseSpeed += 1.0f;
            OnGorillaEnraged?.Invoke();
        }
    }

    private void UpdateTimersAndWeights()
    {
        if (meleeTimer > 0) meleeTimer -= Time.deltaTime;
        if (specialAttackTimer > 0) specialAttackTimer -= Time.deltaTime;
        if (verticalNavTimer > 0) verticalNavTimer -= Time.deltaTime;

        if (!isBossRaged)
        {
            float healthLost = 1f - bossHealth.GetCurrentHealthPercent();
            int segments = Mathf.FloorToInt(healthLost / 0.05f);
            currentSlamWeight = Mathf.Clamp(segments * 4, 0, 40);
            currentPowerWeight = Mathf.Clamp(segments * 3, 0, 30);
            currentRockWeight = 100 - (currentSlamWeight + currentPowerWeight);
        }
    }

    private void StartJumpSlam()
    {
        isSlamming = true;
        anim.SetTrigger("jumpSlamAttack");
        rib.linearVelocity = new Vector2(rib.linearVelocity.x, jumpSlamHeight);
    }

    private void ExecuteSlamImpact()
    {
        CameraController.Instance.ShakeCamera(0.5f);
        rib.linearVelocity = Vector2.zero;
        if (slamWaveManagerPrefab != null)
        {
            GameObject mgr = Instantiate(slamWaveManagerPrefab, transform.position, Quaternion.identity);
            mgr.GetComponent<SlamWaveManager>().StartWave(new Vector3(transform.position.x, boxCollider.bounds.min.y, 0));
        }
        anim.SetTrigger("landed");
        Invoke(nameof(ResetEylemler), slamRecoveryTime);
    }

    private void ResetEylemler() { isSlamming = false; isAttacking = false; }

    public void TriggerFailingFall()
    {
        if (isDying) return;
        isDying = true;
        state = State.Stasis;
        rib.linearVelocity = new Vector2(0, rib.linearVelocity.y);
        anim.SetBool("isRunning", false);
        anim.SetTrigger("die");
    }

    public void Event_FinishAttack() { if (!isSlamming) isAttacking = false; }

    public void Event_ThrowRock()
    {
        if (rockPrefab == null || throwPoint == null) return;
        GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
        float dir = movingRight ? 1f : -1f;
        rock.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(dir * throwForce, 4f);
    }
}