using UnityEngine;

public class TheEnd : MonoBehaviour
{
    public float duration = 3f; // Her bir hedefe odaklanma süresi
    public Transform targets; // Odaklanılacak hedeflerin listesi

    public Transform player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Tetikleyiciyi kapat ki tekrar tekrar çalışmasın
            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }
            targets.position = player.position;
        }
    }
}