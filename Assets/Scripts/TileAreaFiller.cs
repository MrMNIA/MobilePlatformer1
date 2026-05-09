using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap işlemleri için gerekli

public class TileAreaFiller : MonoBehaviour
{
    [Header("Settings")]
    public Tilemap targetTilemap;   // Doldurulacak Tilemap
    public TileBase tileToPlace;    // Yerleştirilecek Tile (Tile veya RuleTile olabilir)
    public bool deactivateAfterUse = true; // İşlem bitince trigger kapansın mı?

    [Header("Area Coordinates")]
    public Vector3Int startPos;     // Başlangıç koordinatı (x, y, z)
    public Vector3Int endPos;       // Bitiş koordinatı (x, y, z)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FillArea();

            if (deactivateAfterUse)
            {
                if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
                    col.enabled = false;
            }
        }
    }

    public void FillArea()
    {
        if (targetTilemap == null || tileToPlace == null)
        {
            Debug.LogWarning("Tilemap veya Tile seçilmemiş!");
            return;
        }

        // Koordinatların ters girilmesi ihtimaline karşı (min/max hesabı)
        int xMin = Mathf.Min(startPos.x, endPos.x);
        int xMax = Mathf.Max(startPos.x, endPos.x);
        int yMin = Mathf.Min(startPos.y, endPos.y);
        int yMax = Mathf.Max(startPos.y, endPos.y);

        // Belirlenen alanı döngü ile doldur
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                // Z ekseni genelde 2D'de 0'dır
                targetTilemap.SetTile(new Vector3Int(x, y, startPos.z), tileToPlace);
            }
        }

        Debug.Log($"{gameObject.name}: Alan başarıyla dolduruldu.");
    }
}