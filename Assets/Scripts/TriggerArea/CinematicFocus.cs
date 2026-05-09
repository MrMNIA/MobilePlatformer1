using UnityEngine;

public class CinematicFocus : MonoBehaviour
{
    public float duration = 2f; // Her bir hedefe odaklanma süresi
    public Transform[] targets; // Odaklanılacak hedeflerin listesi

    private int currentTargetIndex = 0; // Şu an kaçıncı hedefteyiz?

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tetikleyiciyi kapat ki tekrar tekrar çalışmasın
            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }

            currentTargetIndex = 0; // İndeksi sıfırla
            StartSequence(); // Zinciri başlat
        }
    }

    void StartSequence()
    {
        if (targets == null || currentTargetIndex >= targets.Length)
        {
            // LİSTE BİTTİ: Şimdi kamerayı oyuncuya geri veriyoruz
            CameraController.Instance.EndCinematicFocus();
            return;
        }

        Transform currentTarget = targets[currentTargetIndex];

        // ÖNEMLİ: Son parametreyi 'false' gönderiyoruz. 
        // Böylece kamera her hedef geçişinde oyuncuya dönmeye çalışmaz.
        CameraController.Instance.StartCinematicFocus(currentTarget, 0.25f, duration, false);

        currentTargetIndex++;
        Invoke(nameof(StartSequence), duration);
    }
}