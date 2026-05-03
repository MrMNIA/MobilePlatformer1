using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Advertisements;
using Unity.Services.Core;
using System.Threading.Tasks;

public class ServiceManager : MonoBehaviour, IStoreListener, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{

    public enum AdRewardType { Coin, Respawn }
    private AdRewardType currentRewardType;
    public static ServiceManager Instance;

    
    private IStoreController m_StoreController;
    private int geciciCoinMiktari;
    private int[] paketler = { 1000, 2000, 5000, 10000, 30000 };

    [Header("Ads Settings")]
    [SerializeField] string _androidGameId = "*******";
    [SerializeField] string _adUnitId = "Rewarded_Android";
    [SerializeField] bool _testMode = false;
    [HideInInspector] public bool isAdShowing = false; // Sayaç buraya bakacak
    // Aktif olan dükkan scriptini takip etmek için
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

    async void Start()
    {
        try
        {
            // 1. ADIM: Önce ana servisleri başlat
            await UnityServices.InitializeAsync();
            Debug.Log("UGS Hazır.");

            // 2. ADIM: Reklamı başlat
            Advertisement.Initialize(_androidGameId, _testMode, this);

            // 3. ADIM: IAP için 1 saniye bekle (Çakışmaları önlemek için)
            await Task.Delay(1000);

            // --- KRİTİK KONTROL ---
            // "isInitialized" yerine bunu kullanıyoruz:
            if (m_StoreController != null) return;

            var module = StandardPurchasingModule.Instance();

            // NOT: Eğer 'Transform' hatası devam ederse aşağıdaki satırı // ile yorum satırı yap.
            // Unity 6 bazen bu pencereyi kendisi otomatik oluşturduğu için çift eklemeye çalışıyor.
            module.useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;

            // Builder'ı burada tanımlıyoruz (Initialize'dan önce olmalı!)
            var builder = ConfigurationBuilder.Instance(module);
            foreach (int miktar in paketler)
            {
                builder.AddProduct("gamecoin" + miktar, ProductType.Consumable);
            }

            // 4. ADIM: IAP'yi şimdi başlat
            UnityPurchasing.Initialize(this, builder);
            Debug.Log("IAP Başlatma isteği gönderildi...");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Start Metodunda Hata: " + e.Message);
        }
    }
    // --- SATIN ALMA TETİKLEME ---
    // SATIN ALMA İÇİN:
    // --- SATIN ALMA ---
    public void SatinAl(int miktar)
    {
        geciciCoinMiktari = miktar;
        string urunID = "gamecoin" + miktar;

        // Eğer Test Mode açıksa ve servis yoksa (veya hızlı test istiyorsan) anında ver
        if (_testMode && m_StoreController == null)
        {
            Debug.Log("Jüri Modu: Servis yok, anında ödül veriliyor.");
            ProcessPurchase(null);
            return;
        }

        // Geri kalan her durumda (Editor veya Telefon) marketi zorla
        if (m_StoreController != null)
        {
            m_StoreController.InitiatePurchase(urunID);
        }
        else
        {
            Debug.LogWarning("Market henüz hazır değil!");
        }
    }

    // --- REKLAM ---
    // Altın için çağıracağın metod
    public void WatchADSCoin()
    {
        currentRewardType = AdRewardType.Coin;
        StartAdSequence();
    }

    // Canlanma için çağıracağın metod
    public void WatchADSRespawn()
    {
        currentRewardType = AdRewardType.Respawn;
        StartAdSequence();
    }

    // Reklam başlatma mantığını tek yere topladık
    private void StartAdSequence()
    {
        if (Advertisement.isInitialized)
        {
            Advertisement.Load(_adUnitId, this);
        }
        else if (_testMode) // Servis yoksa ama test modundaysak direkt ödül ver (Jüri için)
        {
            OnUnityAdsShowComplete(_adUnitId, UnityAdsShowCompletionState.COMPLETED);
        }
    }

    // ÖDÜLÜN VERİLDİĞİ YER (BURASI KRİTİK)
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState completionState)
    {
        isAdShowing = false;
        if (adUnitId.Equals(_adUnitId) && completionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            if (currentRewardType == AdRewardType.Coin)
            {
                OdulVer(250);
            }
            else if (currentRewardType == AdRewardType.Respawn)
            {
                // Burada oyuncuyu canlandırma kodunu tetikle
                Debug.Log("Oyuncu canlandırılıyor...");
                UIManager.instance.Respawn();
                // Not: Kendi Respawn metodunu buraya yazmalısın.
            }
        }
    }
    // --- IAP CALLBACKS ---
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        int mevcutCoin = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", mevcutCoin + geciciCoinMiktari);
        PlayerPrefs.Save();

        if (currentShopUI != null)
        {
            currentShopUI.UpdateUI();
            currentShopUI.ShowMessage("Transaction Successful!");
        }

        SoundManager.Instance.PlaySound(MoneyManager.Instance.moneyspendSound);
        return PurchaseProcessingResult.Complete;
    }

    // --- ADS CALLBACKS ---
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (this == null) return;

        Debug.Log("Reklam yüklendi, gösteriliyor...");
        // TimeScale'den etkilenmemesi için direkt gösteriyoruz
        Advertisement.Show(_adUnitId, this);
    }

    void ExecuteShow()
    {
        if (this != null)
        {
            Advertisement.Show(_adUnitId, this);
        }
    }

    void OdulVer(int miktar)
    {
        MoneyManager.Instance.AddCoins(miktar);
        PlayerPrefs.Save();
        if (currentShopUI != null)
        {
            currentShopUI.UpdateUI();
            currentShopUI.ShowMessage("Reward Received!");
        }
        SoundManager.Instance.PlaySound(MoneyManager.Instance.moneyspendSound);
    }

    // --- ZORUNLU INTERFACE METOTLARI ---
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        Debug.Log(">>> MÜJDE: IAP Başarıyla Başlatıldı! Market Kontrolcüsü Hazır.");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError(">>> HATA: IAP Başlatılamadı! Sebep: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($">>> HATA: IAP Başlatılamadı! Sebep: {error}, Mesaj: {message}");
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { if (currentShopUI != null) currentShopUI.ShowMessage("Purchase Canceled."); SoundManager.Instance.PlaySound(MoneyManager.Instance.notenoughSound); }
    public void OnInitializationComplete() { }
    public void OnInitializationFailed(UnityAdsInitializationError error, string message) { }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) { if (currentShopUI != null) currentShopUI.ShowMessage("Ad Load Failed."); SoundManager.Instance.PlaySound(MoneyManager.Instance.notenoughSound); }
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) { isAdShowing = false;}
    public void OnUnityAdsShowStart(string adUnitId) { isAdShowing = true;}
    public void OnUnityAdsShowClick(string adUnitId) { }

    private void OnApplicationQuit()
    {
        Instance = null;
    }
}