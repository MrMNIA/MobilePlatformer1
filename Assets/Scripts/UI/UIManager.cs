using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject tutorialPanel; // Tutorial panelleri için dizi
    public GameObject closeTutorialButton; // Tutorial kapatma butonu

    [Header("HUD Elements")]
    public GameObject pauseButton;
    public Text currentLevelCoinText;
    public Text finalCoinText;
    public Text addText; // Seviye sonu kazançlarını göstermek için (isteğe bağlı)

    [Header("Audio")]
    public AudioClip gameOverSound;
    public AudioClip winSound;


    public static UIManager instance;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        tutorialPanel.SetActive(false);
        for (int i = 0; i < tutorialPanel.transform.childCount; i++)
        {
            tutorialPanel.transform.GetChild(i).gameObject.SetActive(false); // Tüm tutorial panellerini gizle
        }
        closeTutorialButton.SetActive(false);

        UpdateCurrentLevelCoinUI(0);
    }

    public void UpdateCurrentLevelCoinUI(int amount)
    {
        if (currentLevelCoinText != null)
            currentLevelCoinText.text = amount.ToString();
    }
    public void PauseGame()
    {
        SoundManager.Instance.PauseMusic();
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pauseButton.SetActive(false); // Oyun duruyorken pause butonu gizlensin
    }

    // 2. Devam Etme (Pause menüsündeki Resume butonuna basınca çalışır)
    public void ResumeGame()
    {
        SoundManager.Instance.ResumeMusic();
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        pauseButton.SetActive(true); // Pause butonu geri gelsin
    }

    public void ShowTutorial(int tutorialNumber)
    {
        tutorialPanel.SetActive(true);
        foreach (Transform child in tutorialPanel.transform)
        {
            child.gameObject.SetActive(false); // Tüm tutorial panellerini gizle
        }
        tutorialPanel.transform.GetChild(tutorialNumber).gameObject.SetActive(true);
        closeTutorialButton.SetActive(true);
    }
    public void CloseTutorial()
    {
        SoundManager.Instance.PlaybuttonClickSound();
        tutorialPanel.SetActive(false);
        closeTutorialButton.SetActive(false); // Kapatma butonunu gizle

    }
    public void RestartGame()
    {
        // ÇOK ÖNEMLİ: Zamanı tekrar akıtmalısın! 
        // Yoksa sahne yüklendiğinde oyun 0 hızında (donuk) başlar.
        Time.timeScale = 1f;

        // Mevcut aktif sahnenin adını veya index'ini alıp tekrar yüklüyoruz
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Eğer bir sonraki sahne build settings'de varsa yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f; // Zamanı normalleştir
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Eğer sonraki seviye yoksa ana menüye dön
            GotoMainMenu();
        }
    }

    public void GotoMainMenu()
    {
        Time.timeScale = 1f; // Ana menüye giderken zamanı normalleştir
        SceneManager.LoadScene(0); // Ana menü sahnesinin adını kullanarak yükle
    }


    // 3. Oyun Sonu (Ölünce çağrılır)
    public void ShowGameOver()
    {
        SoundManager.Instance.PlaySound(gameOverSound);
        SoundManager.Instance.PauseMusic();
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);
    }

    // 4. Kazanma (Win Zone'a girince çağrılır)
    public IEnumerator ShowWinScreen(int baseAmount, bool isFirstClear)
    {
        SoundManager.Instance.PlaySound(winSound);

        // Zamanı durdurduğunuz için 'Realtime' bekleme kullanmalıyız
        Time.timeScale = 0f;
        winPanel.SetActive(true);
        pauseButton.SetActive(false);

        // Bekleme süresini değişkene atayalım (Performans için)
        float waitTime = 1f / baseAmount; // Miktara göre hızlanır
        var wait = new WaitForSecondsRealtime(waitTime);

        // 1. AŞAMA: Temel Miktar Artışı
        for (int i = 0; i <= baseAmount; i++)
        {
            finalCoinText.text = i.ToString();
            yield return wait; // Bekleme burada gerçekleşir
        }

        int tempAmount = baseAmount;
        yield return new WaitForSecondsRealtime(0.5f); // Küçük bir duraklama ekleyelim

        // 2. AŞAMA: Zorluk Bonusu
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.currentDifficulty != DifficultyManager.Difficulty.Easy)
        {
            baseAmount = Mathf.RoundToInt(baseAmount * DifficultyManager.Instance.GetCoinMultiplier());

            if (DifficultyManager.Instance.currentDifficulty == DifficultyManager.Difficulty.Medium)
            {
                addText.text = "Medium Bonus: x1.25";
                addText.color = Color.yellow;
            }
            else if (DifficultyManager.Instance.currentDifficulty == DifficultyManager.Difficulty.Hard)
            {
                addText.text = "Hard Bonus: x1.5";
                addText.color = Color.red;
            }

            waitTime = 1f / baseAmount; // Miktara göre hızlanır
            wait = new WaitForSecondsRealtime(waitTime);

            for (int i = tempAmount; i <= baseAmount; i++)
            {
                finalCoinText.text = i.ToString();
                yield return wait;
            }

            tempAmount = baseAmount;
        }

        yield return new WaitForSecondsRealtime(0.5f); // Küçük bir duraklama ekleyelim

        // 3. AŞAMA: İlk Bitirme Bonusu
        if (isFirstClear)
        {
            addText.text = "First Clear Bonus: x2"; // Alt satıra geçmesi için \n ekledim
            addText.color = Color.green;
            baseAmount = Mathf.RoundToInt(baseAmount * 2);

            waitTime = 1f / baseAmount; // Miktara göre hızlanır
            wait = new WaitForSecondsRealtime(waitTime);

            for (int i = tempAmount; i <= baseAmount; i++)
            {
                finalCoinText.text = i.ToString();
                yield return wait;
            }
        }
    }
}