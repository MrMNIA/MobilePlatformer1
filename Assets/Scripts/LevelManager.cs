using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public int currentLevelCoins = 0;

    private void Awake() { Instance = this; }

    public void AddCoins(int baseAmount)
    {
        float multiplier = DifficultyManager.Instance.GetCoinMultiplier();
        currentLevelCoins += Mathf.RoundToInt(baseAmount * multiplier);
        UIManager.instance.UpdateCurrentLevelCoinUI(currentLevelCoins);
    }

    // --- KRİTİK FONKSİYON: Parayı Bankaya Yatır ---
    public void FinalizeLevelCoins(string levelName)
    {
        int totalWallet = PlayerPrefs.GetInt("TotalCoins", 0);
        int bonusMultiplier = 1;

        // Bu level daha önce bitirildi mi? (0: Hayır, 1: Evet)
        string levelKey = levelName + "_Completed";
        if (PlayerPrefs.GetInt(levelKey, 0) == 0)
        {
            bonusMultiplier = 3; // İLK BİTİRME BONUSU!
            PlayerPrefs.SetInt(levelKey, 1); // Artık bitirildi olarak işaretle
            Debug.Log("İLK BİTİRME! x3 Bonus uygulandı.");
        }

        // Hesapla ve Ekle
        int finalAmount = currentLevelCoins * bonusMultiplier;
        totalWallet += finalAmount;

        // Kalıcı olarak kaydet
        PlayerPrefs.SetInt("TotalCoins", totalWallet);
        PlayerPrefs.Save();

        Debug.Log($"Bölüm Sonu: {currentLevelCoins} x {bonusMultiplier} = {finalAmount} bankaya yattı.");
    }
}