using UnityEngine;
using UnityEngine.Tilemaps;

public class LavaGenerator : MonoBehaviour
{
    [Header("Gereksinimler")]
    public Tilemap groundTilemap; // Referans olarak 'Ground' Tilemap'ini sürükle
    public Tilemap lavaTilemap;   // Lav tile'larını koyacağımız yeni bir Tilemap oluşturup buraya sürükle
    public TileBase lavaTile;      // Boyanacak lav tile'ını buraya sürükle

    [Header("Ayarlar")]
    public int horizontalOffset = 2; // Sağdan ve soldan ne kadar taşsın?
    public int verticalOffset = 3;   // Haritanın ne kadar aşağısından başlasın?
    public int lavaHeight = 10;      // Kaç katman lav olsun?

    [ContextMenu("Lava Oluştur")] // Inspector'da script ismine sağ tıklayıp çalıştırabilirsin
    public void GenerateLava()
    {
        if (groundTilemap == null || lavaTile == null)
        {
            Debug.LogError("Lütfen Tilemap ve Lava Tile referanslarını atayın!");
            return;
        }

        groundTilemap.CompressBounds();
        // 1. Mevcut Ground tilemap'inin sınırlarını (en sol, sağ, aşağı) buluyoruz
        BoundsInt bounds = groundTilemap.cellBounds;

        lavaTilemap.ClearAllTiles();

        // 2. Başlangıç ve bitiş noktalarını hesaplıyoruz
        int startX = bounds.xMin - horizontalOffset;
        int endX = bounds.xMax + horizontalOffset;
        int topY = bounds.yMin - verticalOffset; // Ground'un en alt noktasından aşağı offset
        int bottomY = topY - lavaHeight;         // 10 katman aşağısı

        // 3. Belirlenen alanı lav ile boyuyoruz
        for (int x = startX; x < endX; x++)
        {
            for (int y = bottomY; y < topY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                lavaTilemap.SetTile(pos, lavaTile);
            }
        }

        Debug.Log("Lav gridi başarıyla oluşturuldu!");
    }

    private void Start() {
        GenerateLava(); // Oyun başladığında lavı otomatik oluşturmak istersen bu satırı açabilirsin. İstersen sadece Inspector'dan manuel
    }
}