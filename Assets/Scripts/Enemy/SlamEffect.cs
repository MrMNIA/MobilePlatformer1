using UnityEngine;

public class SlamEffect : Dealingdamage
{
    [Header("Shockwave Settings")]
    public float expandSpeed = 10f;
    public float maxScale = 5f;
    public float duration = 0.5f;

    void Start()
    {
        // Oluştuğu an ömrü başlar
        Destroy(gameObject, duration);
        transform.localScale = new Vector3(0.1f, 1f, 1f); // Çok ince başla
    }

    void Update()
    {
        // Etki alanını yatayda hızla genişlet
        if (transform.localScale.x < maxScale)
        {
            transform.localScale += new Vector3(expandSpeed * Time.deltaTime, 0, 0);
        }
    }
}