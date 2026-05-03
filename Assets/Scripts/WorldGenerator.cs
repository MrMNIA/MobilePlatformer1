using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Referanslar")]
    public Tilemap groundTilemap;
    public Tilemap lavaTilemap;
    public TileBase groundFillTile; // Yerin altına döşenecek toprak/kaya
    public TileBase lavaTile;       // Lav tile'ı

    [Header("Yer Dolgusu Ayarları")]
    public int extraFillDepth = 1; // Lavın altından ne kadar daha aşağı insin?

    [Header("Lav Ayarları")]
    public int horizontalOffset = 2; // Sağdan ve soldan taşma miktarı
    public int verticalOffset = 3;   // Yerin ne kadar altından başlasın?
    public int lavaHeight = 10;      // Lavın dikey kalınlığı

    [ContextMenu("Dünyayı Oluştur (Hatasız Hesaplama)")]
    public void GenerateWorld()
    {
        if (groundTilemap == null || lavaTilemap == null || groundFillTile == null || lavaTile == null) return;

        // 1. ADIM: HESAPLAMALARI BAŞTA YAP (Snapshot Al)
        // Sadece senin elle çizdiğin orijinal haritanın sınırlarını baz alıyoruz.
        groundTilemap.CompressBounds();
        BoundsInt originalBounds = groundTilemap.cellBounds;

        // Lavın konumlarını, yerler henüz doldurulmadan ÖNCE hesaplıyoruz.
        int lavaTopY = originalBounds.yMin - verticalOffset;
        int lavaBottomY = lavaTopY - lavaHeight;

        // Dolgu limitini de orijinal sınıra göre belirliyoruz.
        int fillLimitY = originalBounds.yMin - (verticalOffset + extraFillDepth);

        // 3. ADIM: YER DOLGUSU (Aşağıdan Yukarı)
        // Burada originalBounds.xMin/Max kullanıyoruz, böylece döngü sabit kalıyor.
        for (int x = originalBounds.xMin; x < originalBounds.xMax; x++)
        {
            for (int y = fillLimitY; y < originalBounds.yMax; y++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, 0);

                if (groundTilemap.HasTile(currentPos))
                {
                    for (int fillY = fillLimitY; fillY < y; fillY++)
                    {
                        Vector3Int fillPos = new Vector3Int(x, fillY, 0);
                        if (!groundTilemap.HasTile(fillPos))
                        {
                            groundTilemap.SetTile(fillPos, groundFillTile);
                        }
                    }
                    break;
                }
            }
        }

        // 4. ADIM: LAV OLUŞTURMA
        // Artık yukarıda önceden hesapladığımız sabit değerleri kullanıyoruz.
        int startX = originalBounds.xMin - horizontalOffset;
        int endX = originalBounds.xMax + horizontalOffset;

        for (int x = startX; x < endX; x++)
        {
            for (int y = lavaBottomY; y < lavaTopY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                lavaTilemap.SetTile(pos, lavaTile);
            }
        }

        Debug.Log("Dünya senkronize bir şekilde oluşturuldu!");
    }
    private void Start()
    {
        // İstersen oyun başında da tetikleyebilirsin
        GenerateWorld();
    }
}