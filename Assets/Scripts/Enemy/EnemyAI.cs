using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    protected enum State { Patrol, Idle, Chase }

    [Header("Economy & Combat Settings")] // --- YENİ EKLENEN KISIM ---
    public int baseCoinReward = 10; // Öldüğünde vereceği temel coin miktarı
    [Header("AI Movement Settings")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float idleDuration = 1f;
    protected float idleTimer;

    [Header("Detection Settings")]
    public float rayDistance = 1f;
    public LayerMask groundLayer;
    public Vector2 detectionRange = new Vector2(5f, 2f);

    [Header("Range Settings")]
    // Düşmanın durup bekleyeceği mesafe (Oyuncunun içine girmemesi için)
    public float stopDistanceX = 1.5f;
    public float stopDistanceY = 2.0f;

    // Düşmanın saldırıya başlayabileceği gerçek menzil
    public float attackRangeX = 5f;
    public float attackRangeY = 2f;

    protected Transform player;
    protected Rigidbody2D rib;
    protected BoxCollider2D boxCollider;
    protected Animator anim;

    protected bool movingRight = true;
    protected State state;
    protected bool isAggressive = false;

    protected virtual void Awake()
    {
        rib = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        state = State.Patrol;

        GameObject temp = GameObject.FindGameObjectWithTag("Player");
        if (temp != null) player = temp.transform;
    }

    protected virtual void Update()
    {
        if (player == null || !player.gameObject.activeSelf)
        {
            rib.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
            state = State.Patrol;
            return;
        }

        bool isBlocked = CheckIsBlocked();
        // Chase durumundaysak algılama mesafesini %50 artır (Düşman daha zor pes eder)
        float detectionMultiplier = (state == State.Chase) ? 1.5f : 1f;

        bool PlayerInSight = CheckPlayer(detectionMultiplier) || isAggressive;

        switch (state)
        {
            case State.Patrol:
                HandlePatrol(isBlocked);
                if (PlayerInSight) { state = State.Chase; }
                break;
            case State.Idle:
                HandleIdle(isBlocked);
                if (PlayerInSight) { state = State.Chase; }
                break;
            case State.Chase:
                float distanceX = Mathf.Abs(transform.position.x - player.position.x);
                HandleChase(distanceX);
                if (!PlayerInSight && !isAggressive)
                {
                    state = State.Patrol;
                }
                break;
        }
    }

    protected bool CheckPlayer(float multiplier = 1f)
{
    float distX = Mathf.Abs(transform.position.x - player.position.x);
    float distY = Mathf.Abs(transform.position.y - player.position.y);

    // Çarpanı (multiplier) burada kullanıyoruz
    if (distX < detectionRange.x * multiplier && distY < detectionRange.y)
    {
        Vector2 startPos = boxCollider.bounds.center;
        Vector2 endPos = new Vector2(player.position.x, player.position.y + 0.5f); 

        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, groundLayer);
        
        if (hit.collider == null) 
        {
            Debug.DrawLine(startPos, endPos, Color.green);
            return true; 
        }
        else 
        {
            Debug.DrawLine(startPos, endPos, Color.red);
            return false; // Duvara çarptığı için görmüyor
        }
    }
    return false;
}

    protected void HandleChase(float distanceX)
    {
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);
        float xDiff = player.position.x - transform.position.x;

        // Yön değiştirme (Flip)
        if (Mathf.Abs(xDiff) > 0.2f)
        {
            if (xDiff > 0 && !movingRight) Flip();
            else if (xDiff < 0 && movingRight) Flip();
        }

        // MANTIK AYRIMI:
        // 1. Saldırı yapabilir miyim? (Attack Range içinde mi?)
        bool canAttack = (distanceX <= attackRangeX) && (distanceY <= attackRangeY);

        // 2. Durmalı mıyım? (Stop Distance içinde mi?)
        bool shouldStop = (distanceX <= stopDistanceX) && (distanceY <= stopDistanceY);

        // 3. Önüm kapalı mı?
        bool isBlockedAhead = CheckIsBlocked();

        if (!shouldStop && !isBlockedAhead)
        {
            // Durma mesafesinde değilsek ve önümüz boşsa yürümeye devam et
            anim.SetBool("isRunning", true);
            float direction = movingRight ? 1f : -1f;
            rib.linearVelocity = new Vector2(chaseSpeed * direction, rib.linearVelocity.y);
        }
        else
        {
            // Durma mesafesine girdik veya önümüz kapandı, dur.
            rib.linearVelocity = new Vector2(0, rib.linearVelocity.y);
            anim.SetBool("isRunning", false);
        }

        // Eğer saldırı menzilindeysek saldır (Durup durmamaktan bağımsız)
        if (canAttack)
        {
            Attack();
        }
    }
    protected bool CheckIsBlocked()
    {
        float OffsetX = boxCollider.bounds.extents.x + 0.1f;
        if (!movingRight) OffsetX *= -1;

        Vector2 rayOrigin = new Vector2(
            boxCollider.bounds.center.x + OffsetX,
            boxCollider.bounds.center.y - boxCollider.bounds.extents.y
        );

        RaycastHit2D groundInfo = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer);
        RaycastHit2D wallInfo = Physics2D.Raycast(boxCollider.bounds.center, new Vector2(transform.localScale.x, 0), boxCollider.bounds.extents.x + 0.2f, groundLayer);

        return (groundInfo.collider == false || wallInfo.collider == true);
    }

    protected void Flip()
    {
        movingRight = !movingRight;
        Vector3 temp = transform.localScale;
        temp.x *= -1;
        transform.localScale = temp;
    }

    protected void HandlePatrol(bool isBlocked)
    {
        if (isBlocked)
        {
            rib.linearVelocity = Vector2.zero;
            state = State.Idle;
            idleTimer = idleDuration;
        }
        else
        {
            anim.SetBool("isRunning", true);
            if (IsGrounded())
                rib.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rib.linearVelocity.y);
        }
    }

    protected void HandleIdle(bool isBlocked)
    {
        anim.SetBool("isRunning", false);
        if (!isBlocked)
        {
            state = State.Patrol;
            return;
        }
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            Flip();
            state = State.Patrol;
        }
    }

    protected bool IsGrounded()
    {
        Vector2 boxSize = new Vector2(boxCollider.bounds.size.x * 0.9f, 0.1f);
        float distance = boxCollider.bounds.extents.y + 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0f, Vector2.down, distance, groundLayer);
        return hit.collider != null;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // 1. Algılama Menzili (SARI)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange.x * 2, detectionRange.y * 2, 0));

        // 2. Gerçek Saldırı Menzili (KIRMIZI)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeX * 2, attackRangeY * 2, 0));

        // 3. Durma/Fren Mesafesi (MAVİ)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(stopDistanceX * 2, stopDistanceY * 2, 0));
    }


    public virtual void Attack() { }
}