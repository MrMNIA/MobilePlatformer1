using UnityEngine;
using UnityEngine.SceneManagement;
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("Level İçi Para")]
    public int currentLevelCoins = 0;

    [Header("SFX")]
    public AudioClip moneyspendSound;
    public AudioClip notenoughSound;

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

    public void AddCoins(int amount)
    {
        currentLevelCoins += amount;

        // Seviye içindeki geçici UI'ı güncelle
        if (UIManager.instance != null)
            UIManager.instance.UpdateCurrentLevelCoinUI(currentLevelCoins);
    }

    /// <summary>
    /// Seviye bittiğinde parayı hesaplar, çarpanları uygular ve kalıcı cüzdana ekler.
    /// </summary>
    public void FinalizeLevelCoins(string levelName)
    {
        // 1. Temel miktar (Seviye içinde toplanan)
        float earnedAmount = currentLevelCoins;
        // seviye numarasına göre ilave para
        int index = SceneManager.GetActiveScene().buildIndex;
        int levelNum = 10 + index * 5; // Örneğin, her seviye için 10 + (seviye numarası * 5) ekleyebilirsiniz.
        if (index % 10 == 0)
            levelNum += 50 * (index / 10); // Her 10. seviyede ekstra bonus

        earnedAmount += levelNum;
        // 2. Zorluk Çarpanını uygula (Sadece seviye kazancına)
        if (DifficultyManager.Instance != null)
        {
            earnedAmount *= DifficultyManager.Instance.GetCoinMultiplier();
        }

        // 3. İlk bitirme bonusu kontrolü (Sadece seviye kazancına)
        string levelKey = levelName + "_Completed";
        if (PlayerPrefs.GetInt(levelKey, 0) == 0)
        {
            earnedAmount *= 2; // İlk bitirme x2 bonusu
            PlayerPrefs.SetInt(levelKey, 1); // Artık bu seviye bitirildi
        }

        // 4. Nihai miktarı tam sayıya yuvarla
        int finalReward = Mathf.RoundToInt(earnedAmount);

        // 5. Ana cüzdana ekle (Eski para + yeni kazanç)
        int currentTotalWallet = GetTotalCoins();
        SaveTotalCoins(currentTotalWallet + finalReward);

        // 6. Sıfırla ve diske kaydet
        currentLevelCoins = 0;
        PlayerPrefs.Save();
    }

    // --- CÜZDAN VE SHOP YÖNETİMİ ---

    // Cüzdandaki toplam parayı çeker
    public int GetTotalCoins()
    {
        return PlayerPrefs.GetInt("TotalCoins", 0);
    }

    // Harcama yapma fonksiyonu
    public bool TrySpendCoins(int amount)
    {
        int total = GetTotalCoins();

        if (total >= amount)
        {
            total -= amount;
            SaveTotalCoins(total);
            SoundManager.Instance.PlaySound(moneyspendSound);
            PlayerPrefs.Save();
            return true;
        }
        else
        {
            SoundManager.Instance.PlaySound(notenoughSound);
            return false;
        }
    }

    // Parayı kaydeder ve UI'ı (varsa) bilgilendirir
    private void SaveTotalCoins(int amount)
    {
        PlayerPrefs.SetInt("TotalCoins", amount);

        // Eğer ana menüde veya markette toplam parayı gösteren bir UI varsa burayı tetikle:
        // if (UIManager.instance != null) UIManager.instance.UpdateTotalCoinUI(amount);
    }
}