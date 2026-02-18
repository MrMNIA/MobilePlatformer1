using UnityEngine;
using UnityEngine.Tilemaps;

public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public SpriteRenderer layerSprite;
        [Range(0, 1)] public float verticalWeight; // Y ekseni için manuel derinlik

        [HideInInspector] public float initialX;
        [HideInInspector] public float initialY;
        [HideInInspector] public float spriteWidth;
    }

    public Camera cam;
    public Tilemap gridTilemap;
    public ParallaxLayer[] layers;

    private float _initialCamX;
    private float _initialCamY;
    private float _gridStartX, _gridEndX;
    private float _camHalfWidth;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // 1. Başlangıç referanslarını kaydet
        _initialCamX = cam.transform.position.x;
        _initialCamY = cam.transform.position.y;
        _camHalfWidth = cam.orthographicSize * cam.aspect;

        // 2. Grid sınırlarını Tilemap'ten çek
        _gridStartX = gridTilemap.localBounds.min.x + gridTilemap.transform.position.x;
        _gridEndX = gridTilemap.localBounds.max.x + gridTilemap.transform.position.x;

        foreach (var layer in layers)
        {
            if (layer.layerSprite == null) continue;

            // Görselin başlangıç konumunu ve genişliğini kaydet
            layer.initialX = layer.layerSprite.transform.position.x;
            layer.initialY = layer.layerSprite.transform.position.y;
            layer.spriteWidth = layer.layerSprite.bounds.size.x;
        }
    }

    void LateUpdate()
    {
        float currentCamX = cam.transform.position.x;
        float currentCamY = cam.transform.position.y;

        foreach (var layer in layers)
        {
            if (layer.layerSprite == null) continue;

            // --- X EKSENİ: BAŞLANGIÇ NOKTASINDAN SINIRLARA ORANLAMA ---
            float targetX = layer.initialX;

            if (currentCamX > _initialCamX)
            {
                // Kamera başlangıçtan SAĞA gidiyorsa
                float camRightTrack = (_gridEndX - _camHalfWidth) - _initialCamX;
                float spriteRightTrack = (_gridEndX - (layer.spriteWidth / 2f)) - layer.initialX;

                float progress = Mathf.InverseLerp(_initialCamX, _initialCamX + camRightTrack, currentCamX);
                targetX = Mathf.Lerp(layer.initialX, layer.initialX + spriteRightTrack, progress);
            }
            else if (currentCamX < _initialCamX)
            {
                // Kamera başlangıçtan SOLA gidiyorsa
                float camLeftTrack = _initialCamX - (_gridStartX + _camHalfWidth);
                float spriteLeftTrack = layer.initialX - (_gridStartX + (layer.spriteWidth / 2f));

                float progress = Mathf.InverseLerp(_initialCamX, _initialCamX - camLeftTrack, currentCamX);
                targetX = Mathf.Lerp(layer.initialX, layer.initialX - spriteLeftTrack, progress);
            }

            // --- Y EKSENİ: MANUEL DERİNLİK ---
            float camDeltaY = currentCamY - _initialCamY;
            float targetY = layer.initialY + (camDeltaY * layer.verticalWeight);

            // Uygulama
            layer.layerSprite.transform.position = new Vector3(targetX, targetY, layer.layerSprite.transform.position.z);
        }
    }
}