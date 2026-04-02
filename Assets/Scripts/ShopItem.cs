using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Titreme efekti (Coroutine) için gerekli

public class ShopItem : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Text levelText;
    public Text currentValueText;
    public Text plusValueText;
    public Text costText;
    public Button buyButton;

    [Header("Coin UI Ayarları")]
    public GameObject currentCoinsObj; // Senin belirttiğin "currentCoins" objesi
    private Text coinText;             // Bu objenin child'ındaki Text
    private Image coinImage;           // Renk değişimi için (varsa Image bileşeni)
    private Vector3 originalCoinPos;   // Titreme sonrası eski yerine dönmek için

    [Header("Geliştirme Ayarları")]
    public string statName = "Health";
    public int baseValue = 100;
    public int additionPerLevel = 10;
    public int maxLevel = 20;

    [Header("Maliyet Ayarları")]
    public int initialCost = 20;


    private int currentLevel;

    void Awake()
    {
        // Child objedeki Text'i otomatik bulalım
        coinText = currentCoinsObj.GetComponentInChildren<Text>();
        // Eğer objenin kendisinde veya child'ında bir görsel varsa rengini değiştirebilmek için alalım
        coinImage = currentCoinsObj.GetComponent<Image>();
        originalCoinPos = currentCoinsObj.transform.localPosition;
    }

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt(statName + "Level", 0);
        UpdateUI();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void UpdateUI()
    {
        levelText.text = "Lv. " + currentLevel;
        int totalValue = baseValue + (currentLevel * additionPerLevel);
        currentValueText.text = totalValue.ToString();

        // TOPLAM COIN GÜNCELLEME: MoneyManager'dan alıp child text'e yazıyoruz
        if (coinText != null)
        {
            coinText.text = MoneyManager.Instance.GetTotalCoins().ToString();
        }

        if (currentLevel >= maxLevel)
        {
            plusValueText.gameObject.SetActive(false);
            costText.text = "MAX";
            buyButton.interactable = false;
        }
        else
        {
            plusValueText.text = "+" + additionPerLevel;
            costText.text = CalculateCost().ToString();
            plusValueText.gameObject.SetActive(true);
            buyButton.interactable = true;
        }
    }

    int CalculateCost()
    {
        int n = currentLevel;
        
        int cost = Mathf.RoundToInt((2.5f * n * n)) + (10 * n) + 20;

        return cost;
    }

    public void OnBuyClicked()
    {
        int cost = CalculateCost();

        if (MoneyManager.Instance.TrySpendCoins(cost))
        {
            currentLevel++;
            PlayerPrefs.SetInt(statName + "Level", currentLevel);
            PlayerPrefs.Save();
            UpdateUI();
        }
        else
        {
            // PARA YETMEZSE: Efekti başlat
            StopAllCoroutines(); // Eğer zaten titriyorsa çakışmasın
            StartCoroutine(ShakeAndRedFlash());
        }
    }

    // TİTREME VE KIRMIZI YANMA EFEKTİ
    IEnumerator ShakeAndRedFlash()
    {
        float duration = 0.4f; // Efekt süresi
        float magnitude = 5f;  // Titreme şiddeti
        float elapsed = 0f;

        Color originalColor = Color.white; // Varsayılan renk
        if (coinImage != null) originalColor = coinImage.color;

        while (elapsed < duration)
        {
            // Rastgele hafif pozisyon değiştir (Titreme)
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            currentCoinsObj.transform.localPosition = new Vector3(originalCoinPos.x + x, originalCoinPos.y + y, originalCoinPos.z);

            // Rengi kırmızıya yaklaştır
            if (coinImage != null)
                coinImage.color = Color.Lerp(originalColor, Color.red, Mathf.PingPong(elapsed * 10, 1));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Her şeyi eski haline döndür
        currentCoinsObj.transform.localPosition = originalCoinPos;
        if (coinImage != null) coinImage.color = originalColor;
    }
}