using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("Level İçi Para")]
    public int currentLevelCoins = 0;

    private void Awake()
    {
        // Singleton Yapısı
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- PARA KAZANMA VE SEVİYE SONU ---

    public void AddCoins(int baseAmount)
    {
        // DifficultyManager'dan çarpanı al ve ekle
        float multiplier = DifficultyManager.Instance.GetCoinMultiplier();
        currentLevelCoins += Mathf.RoundToInt(baseAmount * multiplier);

        // Seviye içindeki geçici UI'ı güncelle
        if (UIManager.instance != null)
            UIManager.instance.UpdateCurrentLevelCoinUI(currentLevelCoins);
    }

    public void FinalizeLevelCoins(string levelName)
    {
        int totalWallet = GetTotalCoins();
        int bonusMultiplier = 1;

        // İlk bitirme bonusu kontrolü
        string levelKey = levelName + "_Completed";
        if (PlayerPrefs.GetInt(levelKey, 0) == 0)
        {
            bonusMultiplier = 3;
            PlayerPrefs.SetInt(levelKey, 1);
            Debug.Log("İLK BİTİRME! x3 Bonus kazandınız.");
        }

        // Hesapla ve ana cüzdana ekle
        int finalAmount = currentLevelCoins * bonusMultiplier;
        totalWallet += finalAmount;

        SaveTotalCoins(totalWallet);

        // Seviye bittiği için geçici parayı sıfırla
        currentLevelCoins = 0;
    }

    // --- CÜZDAN VE SHOP YÖNETİMİ ---

    // Cüzdandaki toplam parayı PlayerPrefs'ten çeker
    public int GetTotalCoins()
    {
        return PlayerPrefs.GetInt("TotalCoins", 0);
    }

    // Harcama yapmaya çalışır. Para yetiyorsa true döner ve düşer.
    public bool TrySpendCoins(int amount)
    {
        int total = GetTotalCoins();

        if (total >= amount)
        {
            total -= amount;
            SaveTotalCoins(total);
            return true; // İşlem onaylandı
        }

        return false; // Bakiye yetersiz
    }

    // Parayı kaydeder ve UI'ı haberdar eder
    private void SaveTotalCoins(int amount)
    {
        PlayerPrefs.SetInt("TotalCoins", amount);
        PlayerPrefs.Save();
    }
}