using UnityEngine;
using UnityEngine.Tilemaps;

public class DeathZoneManager : MonoBehaviour
{
    [Header("Settings")]
    public Tilemap gridTilemap;
    public float yOffset = -10f;
    public float xPadding = 50f;
    public float scanHeight = 5f; // Tarama alanının dikey kalınlığı
    public LayerMask detectionLayers; // Sadece oyuncu ve düşmanları taramak için

    private Vector2 _boxCenter;
    private Vector2 _boxSize;

    void Start()
    {
        CalculateScanArea();
    }

    void CalculateScanArea()
    {
        if (gridTilemap == null) return;

        // Grid sınırlarını al
        Bounds bounds = gridTilemap.localBounds;
        float gridMinX = bounds.min.x + gridTilemap.transform.position.x;
        float gridMaxX = bounds.max.x + gridTilemap.transform.position.x;
        float gridMinY = bounds.min.y + gridTilemap.transform.position.y;

        // X sınırlarını 50'şer birim genişletiyoruz
        float finalMinX = gridMinX - xPadding;
        float finalMaxX = gridMaxX + xPadding;
        float totalWidth = finalMaxX - finalMinX;

        // Y eşiğini (ölüm çizgisi) belirliyoruz
        float deathY = gridMinY + yOffset;

        // OverlapBox için merkez ve boyut hesapla
        // Kutunun merkezini ölüm çizgisinin biraz altına alıyoruz ki tam üstünden geçeni yakalasın
        _boxCenter = new Vector2((finalMinX + finalMaxX) / 2f, deathY - (scanHeight / 2f));
        _boxSize = new Vector2(totalWidth, scanHeight);
    }

    void FixedUpdate()
    {
        // Alandaki tüm collider'ları bul (Sadece belirli katmanları taramak performansı uçurur)
        Collider2D[] caughtObjects = Physics2D.OverlapBoxAll(_boxCenter, _boxSize, 0f, detectionLayers);

        foreach (Collider2D col in caughtObjects)
        {
            // Eğer objede Health scripti varsa SuddenDeath metodunu çalıştır
            if (col.TryGetComponent(out Health health))
            {
                health.SuddenDeath();
            }
        }
    }

    // Editörde tarama alanını kutu olarak gör
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_boxCenter, _boxSize);
    }
}