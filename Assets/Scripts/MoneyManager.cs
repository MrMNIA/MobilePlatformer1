using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("Level İçi Para")]
    public int currentLevelCoins = 0;

    public GameObject shopWalletUI; // Marketteki cüzdan UI'ı (varsa)

    private Image coinImage;           // Renk değişimi için (varsa Image bileşeni)
    private Vector3 originalCoinPos;   // Titreme sonrası eski yerine dönmek için

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

        // Eğer objenin kendisinde veya child'ında bir görsel varsa rengini değiştirebilmek için alalım
        coinImage = shopWalletUI.GetComponent<Image>();
        originalCoinPos = shopWalletUI.transform.localPosition;
    }

    public void SetShopWallet(GameObject wallet)
    {
        shopWalletUI = wallet;
        UpdateCoins(shopWalletUI); // UI'ı güncelle
    }

    // --- PARA KAZANMA VE SEVİYE SONU ---

    public void UpdateCoins(GameObject uiObject)
    {
        if (uiObject == null) return;

        Text shopWalletText = uiObject.GetComponentInChildren<Text>();
        if (shopWalletText != null)
        {
            shopWalletText.text = GetTotalCoins().ToString();
        }
    }
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

    public bool CheckEnoughCoins(int amount)
    {
        if (GetTotalCoins() >= amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SpendCoins(int amount)
    {
        int total = GetTotalCoins();
        total -= amount;
        SaveTotalCoins(total);
        PlayerPrefs.Save();
    }

    public void WatchADSCoin()
    {
        SoundManager.Instance.PlaySound(moneyspendSound);
        SaveTotalCoins(GetTotalCoins() + 100); // Örneğin, reklam izleyince 100 coin verelim
        PlayerPrefs.Save();
        UpdateCoins(shopWalletUI); // UI'ı güncelle
    }

    public IEnumerator ShakeAndRedFlash(GameObject uiObject)
    {
        float duration = 0.4f;
        float magnitude = 5f;
        float elapsed = 0f;

        SoundManager.Instance.PlaySound(notenoughSound);

        RectTransform rectTransform = uiObject.GetComponent<RectTransform>();
        Vector2 startAnchoredPos = rectTransform.anchoredPosition;

        // Sabit bir coinImage yerine, gönderilen objenin içindeki Image'ı buluyoruz
        Image targetImage = uiObject.GetComponentInChildren<Image>();
        Color originalColor = Color.white;
        if (targetImage != null) originalColor = targetImage.color;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            rectTransform.anchoredPosition = startAnchoredPos + new Vector2(x, y);

            if (targetImage != null)
            {
                float lerpVal = Mathf.PingPong(elapsed * 10, 1);
                targetImage.color = Color.Lerp(originalColor, Color.red, lerpVal);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = startAnchoredPos;
        if (targetImage != null) targetImage.color = originalColor;
    }

    // Parayı kaydeder ve UI'ı (varsa) bilgilendirir
    private void SaveTotalCoins(int amount)
    {
        PlayerPrefs.SetInt("TotalCoins", amount);

        // Eğer ana menüde veya markette toplam parayı gösteren bir UI varsa burayı tetikle:
        // if (UIManager.instance != null) UIManager.instance.UpdateTotalCoinUI(amount);
    }
}