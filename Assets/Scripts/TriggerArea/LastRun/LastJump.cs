using UnityEngine;

public class MultiJumpPad : MonoBehaviour
{
    [Header("Goril Zıplama Ayarları")]
    public float gorillaJumpForce = 18f;
    public float gorillaForwardBoost = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. OYUNCU KONTROLÜ
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.isInSuperJumpZone = true;
            Debug.Log("<color=cyan>[JUMP]:</color> Player Super Jump bölgesine girdi. Zıplama bekleniyor...");
        }

        // 2. GORİL KONTROLÜ
        GorillaBossAI gorilla = other.GetComponent<GorillaBossAI>();
        if (gorilla != null)
        {
            // Goril bu alana girdiği an otomatik olarak zıplar (Kaçış hissi için)
            // Not: Buradaki kuvvetler Goril'in uçurumun yarısında kalmasını sağlayacak şekilde ayarlanmalı.
            gorilla.ExecuteExternalJump(gorillaJumpForce, gorillaForwardBoost);
            Debug.Log("<color=orange>[JUMP]:</color> Goril uçuruma doğru zıplatıldı (ve muhtemelen düşecek).");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Oyuncu bölgeden çıkarsa yetkiyi al
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.isInSuperJumpZone = false;
        }
    }
}