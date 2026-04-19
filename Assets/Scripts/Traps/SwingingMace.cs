using UnityEngine;

public class SwingingMace : MonoBehaviour
{
    [Header("Sallanma Ayarları")]
    public float angleLimit = 45f;    // Maksimum kaç derece sağa/sola sallanacak
    public float baseSpeed = 2f;      // Temel sallanma hızı

    [Header("Görsel Ayarlar")]
    public Transform pivotPoint;      // Güllenin bağlı olduğu tepe noktası

    private float timer = 0f;

    private void Update()
    {
        // 1. Zorluk çarpanını al ve hızı güncelle
        float multiplier = DifficultyManager.Instance.GetStatsMultiplier();
        float adjustedSpeed = baseSpeed * multiplier;

        // 2. Zamanı ilerlet
        timer += Time.deltaTime * adjustedSpeed;

        // 3. Sinüs kullanarak açıyı hesapla
        // Sinüs -1 ile 1 arası değer döndürür, bunu angleLimit ile çarpıyoruz
        float angle = Mathf.Sin(timer) * angleLimit;

        // 4. Objeyi pivot noktası etrafında döndür
        if (pivotPoint != null)
        {
            // Pivot varsa onun etrafında dön
            transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Pivot atanmadıysa kendi etrafında (tepe noktasının burası olduğunu varsayarak) dön
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnDrawGizmos()
    {
        // Editörde sallanma menzilini görelim
        if (pivotPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pivotPoint.position, transform.position);
        }
    }
}