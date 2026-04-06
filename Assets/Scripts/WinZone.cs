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
        if (MoneyManager.Instance == null) return;

        int baseAmount = MoneyManager.Instance.currentLevelCoins;

        // ÖNEMLİ: Daha parayı finalize etmeden "ilk bitirme mi?" kontrolünü yapıyoruz
        string levelKey = levelName + "_Completed";
        bool isFirstClear = PlayerPrefs.GetInt(levelKey, 0) == 0;

        // 1. Muhasebe işlemini yap (Veritabanına/Prefs'e kaydeder)
        MoneyManager.Instance.FinalizeLevelCoins(levelName);

        // 2. UI Ekranını aç (isFirstClear bilgisini de gönderiyoruz)
        if (UIManager.instance != null)
        {
            StartCoroutine(UIManager.instance.ShowWinScreen(baseAmount, isFirstClear));
        }

        // 3. Level tamamlandı bilgisini kaydet (Bu, sonraki levelin kilidini açmak için gerekli)
        PlayerPrefs.SetInt(levelKey, 1); // Level tamamlandı olarak işaretle
        PlayerPrefs.Save();
    }
}