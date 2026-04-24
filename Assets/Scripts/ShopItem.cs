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
    public float baseValue = 100;
    public float additionPerLevel = 10;
    public int maxLevel = 20;

    [Header("Maliyet Ayarları")]
    public int initialCost = 20;
    public int costMultiplier = 10; // Her seviye için maliyeti ne kadar artırmak istediğinizi belirleyin
    private int currentLevel;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt(statName + "Level", 0);
        UpdateUI();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void UpdateUI()
    {
        levelText.text = "Lv. " + currentLevel;
        float totalValue = baseValue + (currentLevel * additionPerLevel);
        currentValueText.text = totalValue.ToString();

        MoneyManager.Instance.UpdateCoins(MoneyManager.Instance.shopWalletUI); // Cüzdan UI'ını güncelle
        
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
        
        int cost = Mathf.RoundToInt((costMultiplier/4f) * n * n) + (costMultiplier * n) + initialCost;

        cost = Mathf.RoundToInt(cost / 10f) * 10;

        return cost;
    }

    public void OnBuyClicked()
    {
        int cost = CalculateCost();

        if (MoneyManager.Instance.CheckEnoughCoins(cost))
        {
            MoneyManager.Instance.SpendCoins(cost);
            SoundManager.Instance.PlaySound(MoneyManager.Instance.moneyspendSound);

            currentLevel++;
            PlayerPrefs.SetInt(statName + "Level", currentLevel);
            PlayerPrefs.Save();
            
            UpdateUI();
        }
        else
        {
            StartCoroutine(MoneyManager.Instance.ShakeAndRedFlash(MoneyManager.Instance.shopWalletUI));
        }
    }

    // TİTREME VE KIRMIZI YANMA EFEKTİ
    
}