using UnityEngine;
using UnityEngine.Tilemaps;

public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public SpriteRenderer layerSprite;
        [Range(0, 1)] public float verticalWeight;

        [HideInInspector] public float initialX;
        [HideInInspector] public float initialY;
        [HideInInspector] public float spriteWidth;
        [HideInInspector] public Vector3 initialScale; // Başlangıç ölçeğini korumak için
    }

    public Camera cam;
    public Tilemap gridTilemap;
    public ParallaxLayer[] layers;

    private float _initialCamX;
    private float _initialCamY;
    private float _gridStartX, _gridEndX;
    private float _initialOrthographicSize; // Başlangıç zoom seviyesi

    void Start()
    {
        if (cam == null) cam = Camera.main;

        _initialCamX = cam.transform.position.x;
        _initialCamY = cam.transform.position.y;
        _initialOrthographicSize = cam.orthographicSize;

        _gridStartX = gridTilemap.localBounds.min.x - 15 + gridTilemap.transform.position.x;
        _gridEndX = gridTilemap.localBounds.max.x + 15 + gridTilemap.transform.position.x;

        foreach (var layer in layers)
        {
            if (layer.layerSprite == null) continue;

            layer.initialX = layer.layerSprite.transform.position.x;
            layer.initialY = layer.layerSprite.transform.position.y;
            layer.spriteWidth = layer.layerSprite.bounds.size.x;
            layer.initialScale = layer.layerSprite.transform.localScale;
        }
    }

    void LateUpdate()
    {
        float currentCamX = cam.transform.position.x;
        float currentCamY = cam.transform.position.y;

        // 1. ADIM: Kamera genişliğini o anki zoom seviyesine göre güncelle
        float currentCamHalfWidth = cam.orthographicSize * cam.aspect;

        // 2. ADIM: Zoom oranını hesapla (Görselleri zoom ile senkronize büyütmek istersen)
        float zoomRatio = cam.orthographicSize / _initialOrthographicSize;

        foreach (var layer in layers)
        {
            if (layer.layerSprite == null) continue;

            // --- X EKSENİ HESABI ---
            float targetX = layer.initialX;

            if (currentCamX > _initialCamX)
            {
                // Mevcut kamera genişliğini kullanarak sağ sınırı hesapla
                float camRightTrack = (_gridEndX - currentCamHalfWidth) - _initialCamX;
                float spriteRightTrack = (_gridEndX - (layer.spriteWidth * zoomRatio / 2f)) - layer.initialX;

                float progress = Mathf.InverseLerp(_initialCamX, _initialCamX + camRightTrack, currentCamX);
                targetX = Mathf.Lerp(layer.initialX, layer.initialX + spriteRightTrack, progress);
            }
            else if (currentCamX < _initialCamX)
            {
                // Mevcut kamera genişliğini kullanarak sol sınırı hesapla
                float camLeftTrack = _initialCamX - (_gridStartX + currentCamHalfWidth);
                float spriteLeftTrack = layer.initialX - (_gridStartX + (layer.spriteWidth * zoomRatio / 2f));

                float progress = Mathf.InverseLerp(_initialCamX, _initialCamX - camLeftTrack, currentCamX);
                targetX = Mathf.Lerp(layer.initialX, layer.initialX - spriteLeftTrack, progress);
            }

            // --- Y EKSENİ HESABI ---
            float camDeltaY = currentCamY - _initialCamY;
            float targetY = layer.initialY + (camDeltaY * layer.verticalWeight);

            // --- UYGULAMA ---
            layer.layerSprite.transform.position = new Vector3(targetX, targetY, layer.layerSprite.transform.position.z);

            // Opsiyonel: Zoom yapıldığında arka planın çok küçük kalmasını engellemek için scale güncelleme
            layer.layerSprite.transform.localScale = layer.initialScale * zoomRatio;
        }
    }
}