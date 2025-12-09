using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    protected enum State { Patrol, Idle, Chase }

    [Header("AI Movement Settings")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float idleDuration = 1f;
    protected float idleTimer;

    [Header("Detection Settings")]
    public float rayDistance = 1f;
    public LayerMask groundLayer;
    public Vector2 detectionRange = new Vector2(5f, 2f);

    [Header("Attack Trigger Settings")]
    // Bu deðerleri inspector'dan elle girmene gerek kalmayacak, 
    // MeleeEnemy scripti bunlarý otomatik yönetecek.
    public float attackRangeX = 1.5f;
    public float attackRangeY = 2.0f;

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
        bool PlayerInSight = CheckPlayer() || isAggressive;

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

    protected bool CheckPlayer()
    {
        float distX = Mathf.Abs(transform.position.x - player.position.x);
        float distY = Mathf.Abs(transform.position.y - player.position.y);
        return (distX < detectionRange.x && distY < detectionRange.y);
    }

    protected void HandleChase(float distanceX)
    {
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);
        float xDiff = player.position.x - transform.position.x;

        if (Mathf.Abs(xDiff) > 0.2f)
        {
            if (xDiff > 0 && !movingRight) Flip();
            else if (xDiff < 0 && movingRight) Flip();
        }

        // Saldýrý pozisyonunda mýyýz?
        // NOT: attackRangeX artýk merkezden merkeze olan toplam mesafeyi ifade edecek.
        bool inAttackPosition = (distanceX <= attackRangeX) && (distanceY <= attackRangeY);

        if (!inAttackPosition)
        {
            anim.SetBool("isRunning", true);
            if (IsGrounded())
            {
                float direction = movingRight ? 1f : -1f;
                rib.linearVelocity = new Vector2(chaseSpeed * direction, rib.linearVelocity.y);
            }
        }
        else
        {
            // Menzile girdik, dur ve saldýr.
            rib.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
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
        // 1. Detection Range (SARI) - Fark etme alaný
        Gizmos.color = Color.yellow;
        // detectionRange merkezden uzaklýk olduðu için boyutu 2 ile çarpýyoruz.
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange.x * 2, detectionRange.y * 2, 0));

        // 2. Attack Trigger Range (MAVÝ) - Durma/Fren yapma alaný
        // Bu kutunun içine Oyuncu girdiði an Enemy koþmayý býrakýr ve saldýrýya geçer.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeX * 2, attackRangeY * 2, 0));
    }

    public virtual void Attack() { }
}