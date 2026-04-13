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

    [Header("Difficulty Info")]
    public GameObject[] difficultyButtons = new GameObject[3];// Zorluk butonları
    public Text difficultyInfoText; // Zorluk seçildiğinde bilgi göstermek için

    [Header("Audio UI References")]
    public Text menuMusicText; // Inspector'dan o sahnedeki Text'i sürükle
    public Text menuSfxText;   // Inspector'dan o sahnedeki Text'i sürükle

    private int selectedLevelName;

    void Start()
    {
        Application.targetFrameRate = 60;
        ShowMainMenu();
        SelectDifficulty((int)DifficultyManager.Instance.currentDifficulty); // Mevcut zorluğu göster
        StartCoroutine(SceneFader.Instance.ManualFadeIn());
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        optionsPanel.SetActive(false);
        shopPanel.SetActive(false); // Mağaza panelini kapat
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        SoundManager.Instance.UpdateDisplay(menuMusicText, menuSfxText);

        SoundManager.Instance.PlaybuttonClickSound();
        shopPanel.SetActive(false); // Mağaza panelini kapat
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

    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        SoundManager.Instance.PlaybuttonClickSound();
        shopPanel.SetActive(false); // Mağaza panelini kapat
    }

    public void OpenDifficultyPopup(int levelIndex)
    {
        selectedLevelName = levelIndex;
        difficultyPanel.SetActive(true);
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

        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            // Butonun üzerindeki Image bileşenini alıyoruz
            Image btnImg = difficultyButtons[i].GetComponent<Image>();

            // Geçerli rengi HSV'ye çevirmek için değişkenler
            float h, s, v;
            Color.RGBToHSV(btnImg.color, out h, out s, out v);

            if (i == difficultyIndex)
            {
                // Seçilen buton: Doygunluğu fulle (Canlı yap)
                v = 1f;
                btnImg.color = Color.HSVToRGB(h, s, v);
            }
            else
            {
                // Diğerleri: Doygunluğu düşür (Griye yaklaştır)
                v = 0.4f;
                btnImg.color = Color.HSVToRGB(h, s, v);
            }
        }

        switch (DifficultyManager.Instance.currentDifficulty)
        {
            case DifficultyManager.Difficulty.Easy:
                difficultyInfoText.color = Color.green;
                difficultyInfoText.text = "Enemy Health = 100% \nEnemy Damage = 100%\nCoin Multiplier = 100%";
                break;
            case DifficultyManager.Difficulty.Medium:
                difficultyInfoText.color = Color.yellow;
                difficultyInfoText.text = "Enemy Health = 125% \nEnemy Damage = 125%\nCoin Multiplier = 125%";
                break;
            case DifficultyManager.Difficulty.Hard:
                difficultyInfoText.color = new Color(153, 0, 0, 255) / 255f; // Koyu kırmızı
                difficultyInfoText.text = "Enemy Health = 150% \nEnemy Damage = 150%\nCoin Multiplier = 150%";
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