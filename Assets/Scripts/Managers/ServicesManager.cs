using UnityEngine;
using UnityEngine.Purchasing;
using GoogleMobileAds.Api; // Google AdMob Namespace
using System;
using System.Collections.Generic;

public class ServiceManager : MonoBehaviour, IStoreListener
{
    public enum AdRewardType { Coin, Respawn }
    private AdRewardType currentRewardType;
    public static ServiceManager Instance;

    private IStoreController m_StoreController;
    private int[] paketler = { 1000, 2000, 5000, 10000, 30000 };

    [Header("Google AdMob Settings")]
    // Test için Google'ın evrensel ödüllü reklam ID'sini koydum. Canlıya çıkarken kendi ID'nizle değiştirin.
    [SerializeField] string _adUnitId = "ca-app-pub-4278847304438026/4841672830";

    private RewardedAd _rewardedAd;

    [HideInInspector] public bool isAdShowing = false;
    [HideInInspector] public CoinShopItem currentShopUI;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        try
        {
            // 1. ADIM: Google Mobile Ads (AdMob) Başlatılması
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                Debug.Log("Google AdMob Başlatıldı.");
                // AdMob initialize olduktan sonra ilk reklamı arka planda yüklemeye başla
                LoadRewardedAd();
            });

            // 2. ADIM: Google Play IAP Yapılandırması ve Başlatılması
            InitializeIAP();
        }
        catch (Exception e)
        {
            Debug.LogError("ServiceManager Başlatma Hatası: " + e.Message);
        }
    }

    private void InitializeIAP()
    {
        if (m_StoreController != null) return;

        var module = StandardPurchasingModule.Instance();
        var builder = ConfigurationBuilder.Instance(module);

        foreach (int miktar in paketler)
        {
            builder.AddProduct("gamecoin" + miktar, ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    // --- GOOGLE PLAY IAP SATIN ALMA AKIŞI ---
    public void SatinAl(int miktar)
    {
        string urunID = "gamecoin" + miktar;

        if (m_StoreController != null)
        {
            m_StoreController.InitiatePurchase(urunID);
        }
        else
        {
            Debug.LogWarning("Market hazır değil!");
            if (currentShopUI != null) currentShopUI.ShowMessage("Store not available.");
        }
    }

    // --- ADMOB REKLAM YÜKLEME (LOAD) ---
    private void LoadRewardedAd()
    {
        // Eski reklam objesi varsa hafızadan temizle
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Ödüllü reklam yükleniyor...");
        var adRequest = new AdRequest();

        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Reklam yükleme başarısız: " + error);
                return;
            }

            Debug.Log("Reklam başarıyla yüklendi.");
            _rewardedAd = ad;
            RegisterEventHandlers(_rewardedAd);
        });
    }

    // --- REKLAM ÇAĞIRMA BUTONLARI ---
    public void WatchADSCoin()
    {
        if (isAdShowing) return;
        currentRewardType = AdRewardType.Coin;
        ShowRewardedAd();
    }

    public void WatchADSRespawn()
    {
        if (isAdShowing) return;
        currentRewardType = AdRewardType.Respawn;
        ShowRewardedAd();
    }

    // --- ADMOB REKLAM GÖSTERME (SHOW) ---
    private void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            isAdShowing = true;
            _rewardedAd.Show((Reward reward) =>
            {
                // ÖDÜL KAZANILDIĞINDA TETİKLENEN LAMBDA FONKSİYONU
                // AdMob reklam başarıyla TAMAMLANDIĞINDA burayı çalıştırır.
                OnAdRewardEarned();
            });
        }
        else
        {
            Debug.LogWarning("Reklam henüz hazır değil, yeniden yükleniyor...");
            if (currentShopUI != null) currentShopUI.ShowMessage("Reklam şu an müsait değil.");
            LoadRewardedAd(); // Reklam yoksa tekrar yüklemeyi dene
        }
    }

    // --- ADMOB ETKİNLİK DİNLEYİCİLERİ (EVENTS) ---
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Reklam kapatıldığında çalışır
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Reklam kapatıldı.");
            isAdShowing = false;
            // Bir sonraki izleme için hemen yeni reklam yükle
            LoadRewardedAd();
        };

        // Reklam hata verdiğinde çalışır
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Reklam gösterim hatası: " + error);
            isAdShowing = false;
            if (currentShopUI != null) currentShopUI.ShowMessage("Reklam gösterilemedi.");
            LoadRewardedAd();
        };
    }

    // --- ÖDÜLÜN DAĞITILDIĞI KISIM ---
    private void OnAdRewardEarned()
    {
        if (currentRewardType == AdRewardType.Coin)
        {
            OdulVer(250);
        }
        else if (currentRewardType == AdRewardType.Respawn)
        {
            Debug.Log("Oyuncu canlandırılıyor...");
            if (UIManager.instance != null) UIManager.instance.Respawn();
        }
    }

    void OdulVer(int miktar)
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.AddCoins(miktar);
        PlayerPrefs.Save();
        if (currentShopUI != null)
        {
            currentShopUI.UpdateUI();
            currentShopUI.ShowMessage("Ödül Alındı!");
        }
        if (SoundManager.Instance != null && MoneyManager.Instance != null)
            SoundManager.Instance.PlaySound(MoneyManager.Instance.moneyspendSound);
    }

    // --- PRODUCTION IAP DOĞRULAMASI (GOOGLE PLAY) ---
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string purchasedProductId = args.purchasedProduct.definition.id;
        string amountString = purchasedProductId.Replace("gamecoin", "");

        if (int.TryParse(amountString, out int satinAlinanMiktar))
        {
            int mevcutCoin = PlayerPrefs.GetInt("TotalCoins", 0);
            PlayerPrefs.SetInt("TotalCoins", mevcutCoin + satinAlinanMiktar);
            PlayerPrefs.Save();

            if (currentShopUI != null)
            {
                currentShopUI.UpdateUI();
                currentShopUI.ShowMessage("İşlem Başarılı!");
            }
            if (SoundManager.Instance != null && MoneyManager.Instance != null)
                SoundManager.Instance.PlaySound(MoneyManager.Instance.moneyspendSound);
        }

        return PurchaseProcessingResult.Complete;
    }

    // --- IAP INTERFACE YÖNETİMİ ---
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        Debug.Log("Google Play IAP Canlı Mod Aktif.");
    }

    public void OnInitializeFailed(InitializationFailureReason error) => Debug.LogError("IAP Hata: " + error);
    public void OnInitializeFailed(InitializationFailureReason error, string message) => Debug.LogError($"IAP Hata: {error}, {message}");

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        if (currentShopUI != null) currentShopUI.ShowMessage("Satın alma iptal edildi.");
        if (SoundManager.Instance != null && MoneyManager.Instance != null)
            SoundManager.Instance.PlaySound(MoneyManager.Instance.notenoughSound);
    }

    private void OnApplicationQuit() { Instance = null; }
}