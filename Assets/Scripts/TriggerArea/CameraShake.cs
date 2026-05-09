using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 2f; // Her bir hedefe odaklanma süresi
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tetikleyiciyi kapat ki tekrar tekrar çalışmasın
            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }

            CameraController.Instance.ShakeCamera(duration);

        }
    }

}