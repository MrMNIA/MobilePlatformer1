using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    protected enum State { Stasis, Patrol, Idle, Chase }

    [Header("Economy & Combat Settings")]
    public int baseCoinReward = 10;

    [Header("One-Way Platform Settings")]
    public bool canAttackFromOneWay = false;
    public LayerMask oneWayPlatformLayer;

    [Header("AI Movement Settings")]
    public bool startInStasis = false;
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float idleDuration = 1f;
    protected float idleTimer;

    [Header("Detection & Memory Settings")]
    public float rayDistance = 1f;
    public LayerMask groundLayer;
    public Vector2 detectionRange = new Vector2(5f, 2f);
    public float memoryDuration = 1.5f;
    protected float memoryTimer;

    [Header("Advanced Movement")]
    public float accelerationForce = 50f;
    public float airAcceleration = 20f;
    public float frictionForce = 40f;
    [SerializeField] protected float deadzoneX = 0.3f;
    private float stuckTimer;

    [Header("Special Behaviors")]
    public bool canFlee = false;
    public bool canJump = false;
    public float jumpForce = 8f;

    [Header("Range Settings")]
    public float stopDistanceX = 1.5f;
    public float stopDistanceY = 2.0f;
    public float attackRangeX = 5f;
    public float attackRangeY = 2f;

    [Header("Audio Settings")]
    public AudioClip noticeSound; // Oyuncuyu fark edince çalacak ses

    public AudioClip introSound;
    protected Transform player;
    protected Rigidbody2D rib;
    protected BoxCollider2D boxCollider;
    protected Animator anim;

    protected bool movingRight = true;
    public bool isAttacking = false;
    protected State state;
    protected bool isAggressive = false;
    public bool canFallOffWhileChasing = false;

    protected LayerMask combinedLayer => groundLayer | oneWayPlatformLayer;

    protected virtual void Awake()
    {
        rib = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        state = startInStasis ? State.Stasis : State.Patrol;
        GameObject temp = GameObject.FindGameObjectWithTag("Player");
        if (temp != null) player = temp.transform;
    }

    protected virtual void Update()
    {
        if (state == State.Stasis) return;

        if (player == null || !player.gameObject.activeSelf)
        {
            ApplyMovement(0);
            anim.SetBool("isRunning", false);
            state = State.Patrol;
            return;
        }

        bool isBlocked = CheckIsBlocked(false);
        float detectionMultiplier = (state == State.Chase) ? 1.5f : 1f;
        bool playerInSight = CheckPlayer(detectionMultiplier) || isAggressive;

        if (playerInSight) memoryTimer = memoryDuration;
        else memoryTimer -= Time.deltaTime;

        bool shouldChase = memoryTimer > 0;

        switch (state)
        {
            case State.Patrol:
                HandlePatrol(isBlocked);
                Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);
                if (shouldChase)
                {
                    state = State.Chase;
                    SoundManager.Instance.PlaySound(noticeSound);
                } 
                break;
            case State.Idle:
                HandleIdle(isBlocked);
                if (shouldChase)
                {
                    state = State.Chase;
                    SoundManager.Instance.PlaySound(noticeSound);
                }
                break;
            case State.Chase:
                Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, false);
                HandleChase();
                if (!shouldChase)
                {
                    state = State.Idle;
                    idleTimer = idleDuration;
                }
                break;
        }
    }

    protected void HandleChase()
    {
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);
        float xDiff = player.position.x - transform.position.x;

        // 1. Hareket Yönü Kararı
        float moveDir = xDiff > 0 ? 1f : -1f;
        bool isTooClose = (distanceX <= stopDistanceX) && (distanceY <= stopDistanceY);

        if (canFlee && isTooClose) moveDir *= -1f;

        // 2. Bakış Yönü (Sadece saldırmıyorken yürüdüğü yöne baksın)
        if (!isAttacking && distanceX > deadzoneX)
        {
            if (moveDir > 0 && !movingRight) Flip();
            else if (moveDir < 0 && movingRight) Flip();
        }

        // 3. Durma ve Hız Hesaplama
        bool shouldStop = (!canFlee && isTooClose) || isAttacking || CheckIsBlocked(canFallOffWhileChasing);
        float targetSpeed = shouldStop ? 0 : (chaseSpeed * moveDir);

        // 4. Zıplama ve Sıkışma
        bool isGrounded = IsGrounded();
        bool isBlockedAhead = CheckIsBlocked(true);

        if (Mathf.Abs(targetSpeed) > 0.1f && rib.linearVelocity.magnitude < 0.2f && isBlockedAhead)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0;

        if (canJump && isBlockedAhead && (isGrounded || stuckTimer > 0.2f))
        {
            rib.linearVelocity = new Vector2(rib.linearVelocity.x, jumpForce);
            stuckTimer = 0;
        }

        ApplyMovement(targetSpeed);

        // 5. Saldırı (Sadece alt sınıf hazırsa oyuncuya dön ve vur)
        bool canAttackMenzil = (distanceX <= attackRangeX) && (distanceY <= attackRangeY);
        if (canAttackMenzil && ReadyToAttack())
        {
            FacePlayer();
            Attack();
        }
    }

    // Alt sınıflar kendi cooldown'larına göre burayı dolduracak
    protected virtual bool ReadyToAttack()
    {
        return !isAttacking;
    }

    protected void ApplyMovement(float targetSpeed)
    {
        float currentAccel;
        bool grounded = IsGrounded();

        if (Mathf.Abs(targetSpeed) > 0.1f)
            currentAccel = grounded ? accelerationForce : airAcceleration * 1.5f;
        else
            currentAccel = grounded ? frictionForce : 5f;

        float newX = Mathf.MoveTowards(rib.linearVelocity.x, targetSpeed, currentAccel * Time.deltaTime);
        rib.linearVelocity = new Vector2(newX, rib.linearVelocity.y);

        anim.SetBool("isRunning", Mathf.Abs(newX) > 0.1f && !isAttacking);
    }

    protected void FacePlayer()
    {
        if (player == null) return;
        float xDiff = player.position.x - transform.position.x;
        if (xDiff > 0 && !movingRight) Flip();
        else if (xDiff < 0 && movingRight) Flip();
    }

    protected bool CheckIsBlocked(bool ignoreLedges)
    {
        float direction = movingRight ? 1f : -1f;
        RaycastHit2D wallInfo = Physics2D.Raycast(boxCollider.bounds.center, Vector2.right * direction, boxCollider.bounds.extents.x + 0.3f, combinedLayer);
        if (ignoreLedges) return wallInfo.collider != null;

        float OffsetX = (boxCollider.bounds.extents.x + 0.1f) * direction;
        Vector2 rayOrigin = new Vector2(boxCollider.bounds.center.x + OffsetX, boxCollider.bounds.center.y - boxCollider.bounds.extents.y);
        RaycastHit2D groundInfo = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, combinedLayer);
        return (groundInfo.collider == null || wallInfo.collider != null);
    }

    protected void HandlePatrol(bool isBlocked)
    {
        if (isBlocked) { ApplyMovement(0); state = State.Idle; idleTimer = idleDuration; }
        else ApplyMovement(movingRight ? moveSpeed : -moveSpeed);
    }

    protected void HandleIdle(bool isBlocked)
    {
        ApplyMovement(0);
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0) { Flip(); state = State.Patrol; }
    }

    protected bool CheckPlayer(float multiplier = 1f)
    {
        if (player == null) return false;
        float distX = Mathf.Abs(transform.position.x - player.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);
        if (distX < detectionRange.x * multiplier && distY < detectionRange.y)
        {
            LayerMask visionMask = canAttackFromOneWay ? groundLayer : combinedLayer;
            RaycastHit2D hit = Physics2D.Linecast(boxCollider.bounds.center, (Vector2)player.position + Vector2.up * 0.5f, visionMask);
            return hit.collider == null;
        }
        return false;
    }

    protected void Flip()
    {
        movingRight = !movingRight;
        Vector3 temp = transform.localScale;
        temp.x *= -1;
        transform.localScale = temp;
    }

    protected virtual bool IsGrounded()
    {
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.8f, 0.1f);
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0f, Vector2.down, boxCollider.bounds.extents.y + 0.1f, combinedLayer);
        return hit.collider != null;
    }

    public virtual void Attack()
    {
        if (isAttacking) return;
        isAttacking = true;
        rib.linearVelocity = new Vector2(0, rib.linearVelocity.y);
    }

    public void WakeUp() { if (state == State.Stasis) state = State.Idle; }
    public void MakeAggressive() => isAggressive = true;
    public void ShowIntro() { ApplyMovement(0); anim.SetTrigger("intro"); SoundManager.Instance.PlaySound(introSound) ;Invoke(nameof(EndIntro), 1f); }
    public void EndIntro() { }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange.x * 2, detectionRange.y * 2, 0));
        Gizmos.color = Color.red; Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeX * 2, attackRangeY * 2, 0));
        Gizmos.color = Color.blue; Gizmos.DrawWireCube(transform.position, new Vector3(stopDistanceX * 2, stopDistanceY * 2, 0));
    }
}