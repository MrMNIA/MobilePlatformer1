using UnityEngine;

public class FireballProjectile : Dealingdamage
{
    [Header("Projectile Movement")]
    [SerializeField] private float speed = 10f; // Ateş topu genelde oktan biraz daha yavaştır
    [SerializeField] private float resetTime = 3f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveDirection;
    private float lifetime;
    private bool hit;

    public void ActivateProjectile(Vector2 _direction)
    {
        moveDirection = _direction;
        gameObject.SetActive(true);
        hit = false;
        lifetime = 0;

        // Ateş topunun yönünü ayarla (Eğer yuvarlak bir sprite ise buna çok gerek yok ama yönlü bir sprite ise gerekli)
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (hit) return;

        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
        {
            gameObject.SetActive(false);
        }
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerStay2D(collision);

        bool isTarget = ((1 << collision.gameObject.layer) & damageLayer.value) != 0;
        bool isGround = ((1 << collision.gameObject.layer) & groundLayer.value) != 0;

        if (isTarget || isGround)
        {
            hit = true;
            // İstersen burada bir "Patlama" efekti (Particle System) instantiate edebilirsin.
            gameObject.SetActive(false);
        }
    }
}