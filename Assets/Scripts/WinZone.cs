using UnityEngine;

public class WinZone : MonoBehaviour
{
    private bool isWon = false;
    [Header("Level Settings")]
    public string levelName = "Level1"; // Her level için farklı isim ver (örn: Level2, Level3)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isWon)
        {
            isWon = true;
            WinGame();
        }
    }

    void WinGame()
    {
        // 1. Muhasebe işlemini yap (Parayı aktar ve Bonusları hesapla)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.FinalizeLevelCoins(levelName);
        }

        // 2. UI Ekranını aç
        if (UIManager.instance != null)
        {
            // Toplam parayı güncelle ki win ekranında doğru görünsün
            UIManager.instance.UpdateTotalCoinUI();
            UIManager.instance.ShowWinScreen();
        }
    }
}