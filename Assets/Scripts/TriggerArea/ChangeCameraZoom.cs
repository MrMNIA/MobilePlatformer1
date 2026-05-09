using UnityEngine;

public class ChangeCameraZoom : MonoBehaviour
{
    // EnemyAI script'ine sahip düşmanları buraya sürüklüyoruz
    public float delay = 0f;

    public float value = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Sadece "Player" girdiğinde işlem yap
        if (other.CompareTag("Player"))
        {

            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }
            Invoke(nameof(ChangeCamera), delay);

        }
    }

    void ChangeCamera()
    {
        CameraController.Instance.AdjustCamSize(value);
    }
}