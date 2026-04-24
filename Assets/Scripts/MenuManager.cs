using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject difficultyPanel;
    public GameObject optionsPanel;
    public GameObject shopPanel; // Yeni: Mağaza paneli
    public GameObject layoutPanel;

    [Header("Difficulty Info")]
    public GameObject[] difficultyButtons = new GameObject[6];// Zorluk butonları
    public Text[] difficultyInfoText = new Text[2]; // Zorluk bilgisi göstermek için

    [Header ("Wallets")]
    public GameObject shopWallet;
    public GameObject difficultyWallet;

    public GameObject[] powerupButtons = new GameObject[3]; // Yeni: Powerup zorluk bilgisi göstermek için
    public GameObject powerupDifficultyInfoText; // Yeni: Powerup zorluk bilgisi göstermek için

    [Header("Audio UI References")]
    public Text menuMusicText; // Inspector'dan o sahnedeki Text'i sürükle
    public Text menuSfxText;   // Inspector'dan o sahnedeki Text'i sürükle

    private int selectedLevelName;
    private int selectedDifficultyPanel = 0;

    void Start()
    {
        Application.targetFrameRate = 60;
        ShowMainMenu();
        SelectDifficulty((int)DifficultyManager.Instance.currentDifficulty); // Mevcut zorluğu göster
        StartCoroutine(SceneFader.Instance.ManualFadeIn());
        SelectPowerup(3); // Başlangıçta hiçbir powerup seçili değil
        MoneyManager.Instance.UpdateCoins(difficultyWallet); // UI'ı güncelle
        MoneyManager.Instance.SetShopWallet(shopWallet); // Mağaza cüzdanını ayarla
    }


    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        optionsPanel.SetActive(false);
        shopPanel.SetActive(false); // Mağaza panelini kapat
        layoutPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        layoutPanel.SetActive(false);
        optionsPanel.SetActive(true);
        SoundManager.Instance.UpdateDisplay(menuMusicText, menuSfxText);

        SoundManager.Instance.PlaybuttonClickSound();
        shopPanel.SetActive(false); // Mağaza panelini kapat
    }

    public void OpenLayout()
    {
        layoutPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }
    public void ChangeMusicVolume(int amount)
    {
        // Önce sesi değiştir
        SoundManager.Instance.AdjustMusic(amount);
        // Sonra bu sahnedeki taze metin kutularını güncelle
        SoundManager.Instance.UpdateDisplay(menuMusicText, menuSfxText);
    }

    public void ChangeSFXVolume(int amount)
    {
        SoundManager.Instance.AdjustSFX(amount);
        SoundManager.Instance.UpdateDisplay(menuMusicText, menuSfxText);
    }

    public void ResetDefault()
    {
        SettingsManager.Instance.ResetToDefault();
        SoundManager.Instance.PlaybuttonClickSound();
    }

    public void SaveLayout()
    {
        SettingsManager.Instance.SaveLayout();
        SoundManager.Instance.PlaybuttonClickSound();
    }

    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        SoundManager.Instance.PlaybuttonClickSound();
        shopPanel.SetActive(false); // Mağaza panelini kapat
    }

    public void OpenDifficultyPopup(int levelIndex)
    {
        difficultyPanel.SetActive(true);
        selectedLevelName = levelIndex;
        if (levelIndex > 3)
        {
            difficultyPanel.transform.GetChild(1).gameObject.SetActive(true); // Zorluk seçeneklerini göster
            difficultyPanel.transform.GetChild(0).gameObject.SetActive(false); // Zorluk seçeneklerini gizle
            selectedDifficultyPanel = 1; // Zorluk paneli seçili
            MoneyManager.Instance.UpdateCoins(difficultyWallet); // UI'ı güncelle
        }
        else
        {
            difficultyPanel.transform.GetChild(0).gameObject.SetActive(true); // Zorluk seçeneklerini göster
            difficultyPanel.transform.GetChild(1).gameObject.SetActive(false); // Zorluk seçeneklerini gizle
            selectedDifficultyPanel = 0; // Zorluk paneli seçili
        }
        
        SelectDifficulty((int)DifficultyManager.Instance.currentDifficulty); // Mevcut zorluğu göster
        SoundManager.Instance.PlaybuttonClickSound();
        shopPanel.SetActive(false); // Mağaza panelini kapat
    }
    public void CloseDifficultyPopup()
    {
        difficultyPanel.SetActive(false);
        SoundManager.Instance.PlaybuttonClickSound();
        shopPanel.SetActive(false); // Mağaza panelini kapat
    }

    public void SelectDifficulty(int difficultyIndex)
    {
        SoundManager.Instance.PlaybuttonClickSound();
        DifficultyManager.Instance.SetDifficulty(difficultyIndex);

        for (int i = 0; i < 3; i++)
        {
            // Butonun üzerindeki Image bileşenini alıyoruz
            Image btnImg = difficultyButtons[i].GetComponent<Image>();
            Image btnImg2 = difficultyButtons[3 + i].GetComponent<Image>();

             // Geçerli rengi HSV'ye çevirmek için değişkenler

            // Geçerli rengi HSV'ye çevirmek için değişkenler
            float h, s, v;
            Color.RGBToHSV(btnImg.color, out h, out s, out v);

            if (i == difficultyIndex)
            {
                // Seçilen buton: Doygunluğu fulle (Canlı yap)
                v = 1f;
                btnImg.color = Color.HSVToRGB(h, s, v);
                btnImg2.color = Color.HSVToRGB(h, s, v);
            }
            else
            {
                // Diğerleri: Doygunluğu düşür (Griye yaklaştır)
                v = 0.4f;
                btnImg.color = Color.HSVToRGB(h, s, v);
                btnImg2.color = Color.HSVToRGB(h, s, v);
            }
        }

        switch (DifficultyManager.Instance.currentDifficulty)
        {
            case DifficultyManager.Difficulty.Easy:
                difficultyInfoText[selectedDifficultyPanel].color = Color.green;
                difficultyInfoText[selectedDifficultyPanel].text = "Enemy Health = 100% \nEnemy Damage = 100%\nCoin Multiplier = 100%";
                break;
            case DifficultyManager.Difficulty.Medium:
                difficultyInfoText[selectedDifficultyPanel].color = Color.yellow;
                difficultyInfoText[selectedDifficultyPanel].text = "Enemy Health = 125% \nEnemy Damage = 125%\nCoin Multiplier = 125%";
                break;
            case DifficultyManager.Difficulty.Hard:
                difficultyInfoText[selectedDifficultyPanel].color = new Color(153, 0, 0, 255) / 255f; // Koyu kırmızı
                difficultyInfoText[selectedDifficultyPanel].text = "Enemy Health = 150% \nEnemy Damage = 150%\nCoin Multiplier = 150%";
                break;
        }
    }

    public void SelectPowerup(int powerupIndex)
    {
        SoundManager.Instance.PlaybuttonClickSound();

        // Text bileşenini bir kez alalım (Sürekli GetComponent yapmak performansı düşürür)
        Text infoText = powerupDifficultyInfoText.GetComponent<Text>();

        // Enum karşılığını bir değişkene alalım (Okunabilirlik için)
        DifficultyManager.CurrentPowerup targetPowerup = (DifficultyManager.CurrentPowerup)powerupIndex;

        // 1. Durum Kontrolü
        bool ayniButonMu = DifficultyManager.Instance.selectedPowerup == targetPowerup;

        if (ayniButonMu)
        {
            // Zaten seçili olana tıklandıysa veya Start'tan "None" (3) olarak çağrıldıysa burası çalışır
            DifficultyManager.Instance.SetPowerupDifficulty((int)DifficultyManager.CurrentPowerup.None);
        }
        else
        {
            // "None" harici bir seçim yapılıyorsa para kontrolü yap
            if (targetPowerup != DifficultyManager.CurrentPowerup.None)
            {
                if (MoneyManager.Instance.CheckEnoughCoins(50))
                {                    
                    DifficultyManager.Instance.SetPowerupDifficulty(powerupIndex);
                }
                else
                {
                    SoundManager.Instance.PlaySound(MoneyManager.Instance.notenoughSound);
                    StartCoroutine(MoneyManager.Instance.ShakeAndRedFlash(difficultyWallet));
                    return; // Para yetersizse fonksiyonu burada kes, görseller değişmesin
                }
            }
            else
            {
                // Eğer dışarıdan doğrudan 3 (None) gönderildiyse paraya bakmadan None yap
                DifficultyManager.Instance.SetPowerupDifficulty((int)DifficultyManager.CurrentPowerup.None);
            }
        }

        // 2. Görsel Güncelleme
        // Seçili olan güçlendirmeyi al (Az önce yukarıda güncelledik)
        var currentSelected = DifficultyManager.Instance.selectedPowerup;

        for (int i = 0; i < powerupButtons.Length; i++)
        {
            Image btnImg = powerupButtons[i].GetComponent<Image>();
            float h, s, v;
            Color.RGBToHSV(btnImg.color, out h, out s, out v);

            // Eğer i. buton şu an seçili olan powerup ise parlat, değilse (veya None ise) karart
            if (currentSelected != DifficultyManager.CurrentPowerup.None && i == (int)currentSelected)
            {
                v = 1f;
            }
            else
            {
                v = 0.4f;
            }

            btnImg.color = Color.HSVToRGB(h, s, v);
        }

        // 3. Bilgi Metni Güncelleme
        switch (currentSelected)
        {
            case DifficultyManager.CurrentPowerup.Speed:
                infoText.color = Color.green;
                infoText.text = "Speed Powerup: \nIncreases movement speed by 50%";
                break;
            case DifficultyManager.CurrentPowerup.Attack:
                infoText.color = Color.red;
                infoText.text = "Attack Powerup: \nIncreases damage dealt by 50%";
                break;
            case DifficultyManager.CurrentPowerup.Shield:
                infoText.color = Color.blue;
                infoText.text = "Shield Powerup: \nReduces damage taken by 50%";
                break;
            default: // None durumu veya geçersiz bir durum
                infoText.color = Color.black;
                infoText.text = "Select a powerup and start with it to game. \n Costs 50 gold.";
                break;
        }
    }

    public void OpenShop()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        optionsPanel.SetActive(false);
        shopPanel.SetActive(true); // Mağaza panelini aç
        SoundManager.Instance.PlaybuttonClickSound();
    }

    public void ShopADS()
    {
        MoneyManager.Instance.WatchADSCoin();
        SoundManager.Instance.PlaybuttonClickSound();
    }

    public void StartGame()
    {
        SoundManager.Instance.PlaybuttonClickSound();
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        // Kararmayı bekle
        yield return StartCoroutine(SceneFader.Instance.ManualFadeOut());
        // Sahneyi yükle
        SceneManager.LoadScene(selectedLevelName);
    }

    // OYUN KAPATILDIĞINDA FADE OUT
    public void QuitGame()
    {
        SoundManager.Instance.PlaybuttonClickSound();
        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        // Kararmayı bekle
        yield return StartCoroutine(SceneFader.Instance.ManualFadeOut());

        Debug.Log("Oyun kapatılıyor...");
        Application.Quit();
    }

    

}